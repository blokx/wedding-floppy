using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Fasterflect;
using Figgle;
using MX.Kitt.Tools.Binding;
using MX.Kitt.Tools.Files;
using MX.Kitt.Tools.Formatting;
using MX.Kitt.Utils.Coding;
using MX.Kitt.Utils.Core;
using MX.Kitt.Utils.Extensions;
using MX.Kitt.Utils.Helper;
using QRCoder;
using RestSharp;
using RestSharp.Extensions;

namespace Blokx.Kitt.Wedding
{
	public class WeddingOptions : DefaultOptions
	{
		[Option(Default = 3)]
		public int? Max { get; set; }
		
		public string[] Guests { get; set; }
	}

	public class WeddingBonbon : DefaultBonbon<WeddingOptions>
	{
		private static IFileSystem _workspace = null;

		private IFileSystem Workspace => _workspace ??= Documents.GetFolder(Config.WORKSPACE);

		// Liste der Dateien, die auf die Disktette kopiert werden
		private static readonly string[] Assets = new[]
		{
			"README.txt",
			"fermi.txt",
			"himmel-hoelle.txt",
			"link.url", // https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop
			// "astronaut.txt",
		};


		public void GenerateAssets()
		{
			// CSV - Kontakte lesen
			// Short.io - personalisierte Urls generieren
			// QrCode - PNG für URL erzeugen

			var ascii1 = CreateAsciiArt("      Just", "RowanCap") + "\n\n" 
			    + CreateAsciiArt("  Married", "RowanCap");
			var ascii2 = CreateAsciiArt("Franzi", "Larry3d") + "\n\n"
				+ CreateAsciiArt("  & Robbi", "Larry3d"); // "Georgia11");

			// http://loveascii.com/hearts3.html
			var ascii3 = Documents.ReadText("res://blokx-kitt/hearts.ascii");

			string generateAsset(FloppyData data, string file)
			{
				try
				{
					var f = Documents.Check($"res://blokx-kitt/{file}");
					var text = f.ReadText();
					text = text.Format(data); // Platzhalter ersetzen
					var fn = f.Name.Replace("Blokx.Kitt.Wedding.res.", "");
					fn = $"floppies/{data.GuestKey}/{fn}";
					Workspace.SaveText(text, fn);
					return fn;
				}
				catch (Exception e)
				{
					Log.Warn(e, "failed to process asset: {0}", file);
				}
				return null;
			}

			var genData = new List<FloppyData>();
			
			IEnumerable<string> generateFiles(CsvData guest)
			{
				var id = guest.Id;
				var url = CreateShortUrl(id.Key, id.Nick, id.Code);
				var qrcode = CreateQRCode(id.Key, url);

				var data = new FloppyData
				{
					AsciiJustMarried = ascii1,
					AsciiFranziRobbi = ascii2,
					AsciiHearts = ascii3,

					GuestName = id.Nick,
					GuestKey = id.Key,
					GuestCode = id.Code,

					ShortUrl = url,
					QrCodeFile = qrcode,
				};

				genData.Add(data);
				
				foreach (var file in Assets)
				{
					yield return generateAsset(data, file);
				}

				// diacritics und umlaute normalisieren
				var normalized = data.GuestName.ToIdentifier("", "", toUpper: null);
				var ascii = CreateAsciiArt(normalized, null);
				var fn = $"floppies/{data.GuestKey}/{data.GuestCode.ToLowerInvariant()}.ascii.txt";
				Workspace.SaveText(ascii, fn);
				yield return fn;
			}

			// CSV lesen
			var guests = ReadGuestData().ToList();
			foreach (var guest in guests)
			{
				// Disketten-Inhalt erzeugen
				Log.Important("processing {0} {1}", guest.Vorname, guest.Nachname);
				var files = generateFiles(guest).ToList();
				Log.Success("- " + string.Join("\n- ", files));
			}

			// Label-Texte erzeugen
			var tpl = Documents.ReadText($"res://blokx-kitt/label.tpl");
			var date = EncodeDate(new DateTime(2021, 10, 1));

			// nach code sortieren => damit findet man die qr codes besser
			var ordered = genData.Ascending(d => d.GuestCode).ToList();
			var labelPlaceholder = ordered.Select(d => new
			{
				Title = "Wedding Floppy",
				Name = d.GuestName,
				Link = d.ShortUrl.Replace("https://", ""),
				Version = "1.0",
				Date = date,
				Copy = "Robert & Franziska Bauer",
			});
			
			var labels = labelPlaceholder.Select(d => tpl.Format(d));
			var txt = string.Join("\n\n\n\n", labels);
			Workspace.SaveText(txt, "labels.txt");

			// QR Codes nochmal in separates Verzeichnis kopieren
			foreach (var guest in ordered)
			{
				Workspace.Copy($"floppies/{guest.GuestKey}/qrcode.png", $"_qrcodes/qr-{guest.GuestCode} ({guest.GuestName}).png");
			}
		}


		public bool FormatFloppy(string label = null, bool fullformat = false)
		{
			var cmd = $"wedding-{(label ?? "fr")}";
			cmd = $"/C format a: /V:{cmd}{(fullformat ? "" : " /Q")}";
			var success = StartProcess("cmd.exe", cmd, null, false);
			if (success)
			{
				Log.Success("format finished");
				return true;
			}
			
			Log.Error("something went wrong");
			return !fullformat && FormatFloppy(label, true);	
		}
		

		public void DeployFiles(bool askName)
		{
			var all = ReadGuestData().ToList();

			bool deployFloppy(CsvData guest)
			{
				var id = guest.Id;
                var name = $"{guest.Vorname} {guest.Nachname}";
                
                // Formatieren
                var ready = ConsoleKitt.AskBool($"Insert floppy for {name} ({id.Code})");
                ready = ready && FormatFloppy(id.Code);
                if (!ready) return false;
                
                // Dateien kopieren
                var files = Workspace.List($"floppies/{id.Key}/").Flatten();
                foreach (var file in files)
                {
	                file.Value.Copy(@"A:\");
	                // Workspace.Copy($"{key}/{file}", @"A:\");
                }
                return true;
			}

			int matchId(string input, GuestId id)
			{
				var match = id.Code.EqualsIgnoreCase(input) ? 10 : 0;
				match += id.Key.EqualsIgnoreCase(input) ? 5 : 0;
				return match;
			}
			
			if (askName)
			{
				while (true)
				{
					var input = ConsoleKitt.Prompt("Name or Code");
					if(input.IsNullOrEmpty()) return;
					
					var match = all.Select(g => (g, matchId(input, g.Id))).Where(x => x.Item2 > 0)
						.Descending(x => x.Item2).Select(x => x.Item1).FirstOrDefault();
					if (match == null)
					{
						Log.Fail("{0} not found.", input);
						continue;
					}
					
					if(!deployFloppy(match)) return;
				}
			}
			
			foreach (var entry in all)
			{
				if (!deployFloppy(entry))
				{
					return;
				}
			}
		}

		public void CopyToFloppy(string source)
		{
			var file = Documents.Check(source);
			file.Copy(@"A:\");
		}


		public string Personalize(string path, CsvData guest)
		{
			var file = Documents.ReadText(path);

			return file;
		}

		public string CreateAsciiArt(string input, string font)
		{
			var fonts = typeof(FiggleFonts).GetProperties().Where(p => typeof(FiggleFont).IsAssignableFrom(p.PropertyType));
			var render = typeof(FiggleFont).Method("Render");
			var ascii = new StringBuilder();
			fonts = fonts.Where(f => font == null || f.Name.EqualsIgnoreCase(font));

			foreach (var prop in fonts)
			{
				try
				{
					var heading = $@"=== {prop.Name} ===";
					var fontObj = prop.GetValue(null);
					var rendered = (string)render.Invoke(fontObj, new object[] { input, null });
					ascii.AppendLineIf(font == null, () => $"\n{heading}\n");
					ascii.AppendLine(rendered);
					ascii.AppendLineIf(font == null);

					if (font != null)
					{
						Log.Important(heading);
						Log.Info(rendered);
					}
				}
				catch (Exception e)
				{
					Log.Warn("font failed: " + prop.Name);
				}
			}
			var result = ascii.ToString();
			return result;
		}

		public void EncodeAll()
		{	
			var guests = ReadGuestData();
			var encoded = guests.Select((g, i) => new
			{
				g.Vorname,
				g.Nachname,
				g.Id.Nick,
				g.Id.Key,
				g.Id.Code,
				g.Id.Index,
				Page = (i / 10) + 1,
				Column = i % 5 > 1 ? 2 : 1,
			}).ToList();

			var tmp = encoded.FormatColumns(heading:true);
			Log.Important("Guest Keys and Codes:");
			Log.Info(tmp);

			var csv = CSV.Serialize(encoded);
			Workspace.SaveText(csv, "codes.csv");
		}
		
		public GuestId EncodeName(CsvData guest)
		{
			var name = guest.Vorname + " " + guest.Nachname;
			var normalized = name.ToIdentifier("", "", toUpper: false).ToLower();
			var coder = new WeddingCoder();
			var idx = 10 + guest.Index; // 137 + guest.Index * 3;
			// var code = coder.EncodeBytes(idx.GetBytes());
			var code = coder.EncodeText(idx.ToString());

			Log.Info("{0}: key={1}, code={2}", name, normalized, code);

			var decode = coder.DecodeText(code);
			var id = new GuestId
			{
				Nick = guest.Rufname.TrimOrNull() ?? guest.Vorname,
				Key = normalized,
				Code = code,
				Index = decode,
			};
			
			return id;
		}

		public string EncodeDate(DateTime? date)
		{
			date = date ?? DateTime.Today;
			var d = RomanLetterCoder.ArabicToRoman(date.Value.Day);
			var m = RomanLetterCoder.ArabicToRoman(date.Value.Month);
			var y = RomanLetterCoder.ArabicToRoman(date.Value.Year);

			var roman = $"{d}.{m}.{y}";
			Log.Success("{0:dd.MM.yyyy} = {1}", date, roman);

			return roman;
		}


		public IEnumerable<CsvData> ReadGuestData()
		{
			// TODO: vernünftige abstaktion schaffen, damit CSVHelper nicht exportiert werden müssen
			
			var csv = Documents.ReadText(Config.GUEST_FILE);
			var data = CSV.Deserialize<CsvData>(csv, o => { o.Delimiter = ","; }).AsEnumerable();

			// nur gäste mit disketten-bügelbild
			data = data.Where(d => !d.Vorname.IsNullOrEmpty())
				.Where(d => d.Diskette == "x");
			
			// index nach Rufname aufsteigend vergeben
			var orderd = data.Ascending(g => g.Rufname).ToArray();
			orderd.ForEach((d, i) => d.Index = i);
			
			var format = data.FormatColumns();
			Log.Info(format);
			var filter = Options.Guests ?? Array.Empty<string>();
			data = filter.Any() ? data.Where(d => filter.Any(f => f.EqualsIgnoreCase(d.Rufname))) : data;
			data = data.Take(Options.Max ?? 1000).ToList();
			data.ForEach(d => d.Id = EncodeName(d));
			return data;
		}


		public string CreateShortUrl(string key, string name, string code)
		{
			// https://developers.short.io/docs/cre
			// https://developers.short.io/reference#linkspost

			// Short-Link: go.blokx.net/{code}
			// Ziel: wedding.blokx.net/?name={name}&code={code}

			var client = new RestClient(Config.SHORTIO_APIURL);
			client.AddDefaultHeader("Authorization", Config.SHORTIO_APIKEY);
			var req = new RestRequest("links", Method.POST, DataFormat.Json);
			var query = $"?guest={name.UrlEncode()}" +
			            $"&code={code.UrlEncode()}" +
			            $"&key={key.UrlEncode()}";

			req.AddParameter("domain", Config.SHORTIO_BASE);
			req.AddParameter("originalURL", $"{Config.SHORTIO_TARGET}/{query}");
			req.AddParameter("path", code);
			req.AddParameter("title", "Wedding Link - " + name);

			req.AddParameter("utmSource", "qrcode");
			req.AddParameter("utmMedium", "floppy");
			req.AddParameter("utmCampaign", "wedding");

			var res = client.Execute(req);

			var json = res.Content.FromJson<ShortIoCreateResult>();
			var url = json.ShortURL;

			if (url == null)
			{
				if (res.Content.Contains("Link already exists"))
				{
					Log.Info("link already exists");
					json.ShortURL = $"{Config.SHORTIO_BASE}/{code}";
				}
				else
				{
					Log.Warn("Failed to create short url. Response:\n{0}", res.Content);
					throw new ApplicationException("Failed to create short url for " + name);	
				}
			}
			
			Log.Debug(res.Content);
			Log.Success("short url for {0}: {1}", name, json.ShortURL);
			return json.ShortURL;
		}


		public string CreateQRCode(string name, string url)
		{
			var gen = new QRCodeGenerator();
			var qurl = new PayloadGenerator.Url(url);
			var data = gen.CreateQrCode(qurl, QRCodeGenerator.ECCLevel.Q);
			var code = new QRCode(data);
			var bitmap = code.GetGraphic(20);

			var target = Workspace.Check($"floppies/{name}/").InternalPath;
			Documents.Check(target, true); // pfad erzeugen, falls nicht vorhanden
			target = $@"{target}qrcode.png";
			bitmap.Save(target, ImageFormat.Png);

			if (Options.Verbose)
			{
				Documents.Open(target);
			}

			return target;
		}
	}

	public class WeddingCoder : BaseCoder
	{
		private const string BASEDIGITS = "AB1CD2EF3GH4JK5LM6NP7QR8STUVWXYZ";
		
		public WeddingCoder() :base(BASEDIGITS.ToUpperInvariant())
		{}
	}

}
