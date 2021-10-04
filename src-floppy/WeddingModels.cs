namespace Blokx.Kitt.Wedding
{
	public class FloppyData
	{
		public string AsciiJustMarried { get; set; }
		public string AsciiFranziRobbi { get; set; }
		public string AsciiHearts { get; set; }

		public string GuestName { get; set; }
		
		public string GuestKey { get; set; }
		public string GuestCode { get; set; }

		internal string QrCodeFile { get; set; }
		internal string ShortUrl { get; set; }
	}

	public class GuestId
	{
		public string Nick { get; set; }
		public string Key { get; set; }
		public string Code { get; set; }
		public string Index { get; set; }

		public override string ToString()
		{
			return $"{Code}[{Nick}]";
		}
	}

	public class CsvData
	{
		public int Index { get; set; }
		public string Nachname { get; set; }
		public string Vorname { get; set; }
		public string Rufname { get; set; }
		public string Diskette { get; set; }
		public int? Alter { get; set; }
		
		public GuestId Id { get; set; }
	}


	public class ShortIoCreateResult
	{
		/*{
			"idString": "lnk_49n_okhPU",
			"path": "xpsmpw",
			"title": null,
			"icon": null,
			"archived": false,
			"originalURL": "http://yourlongdomain.com/yourlonglink",
			"iphoneURL": null,
			"androidURL": null,
			"splitURL": null,
			"expiresAt": null,
			"expiredURL": null,
			"redirectType": null,
			"cloaking": null,
			"source": null,
			"AutodeletedAt" :null,
			"createdAt": "2019-10-11T16:47:06.000Z",
			"updatedAt": "2019-10-11T16:47:06.000Z",
			"DomainId": 15957,
			"OwnerId": 48815,
			"secureShortURL": "https://example.com/xpsmpw",
			"shortURL": "https://example.com/xpsmpw"
		}*/

		public string ShortURL { get; set; }
		public string OriginalURL { get; set; }
	}
}
