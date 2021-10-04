# Wedding Floppy and Homepage

This repository contains the sources created for our wedding floppy project, which was delivered on October 1st, 2021 as a gift for our adorable wedding audience.

All guests are encouraged to take part in our wedding quiz.

## General Idea

The general idea behind this project is as follows:

* A simple website serves as platform to host links to the wedding quiz and also provides access to various media files and probably also to the spotify playlists of the wedding party.
* A 3.5 inch floppy disk, filled with funny, interesting and creative content is gifted to every guest
* the floppy label contains a shortened link (represented in text form and as QR code) to the website
* the link contains a 3-digit code that is used as identifier for all guests

## Some Technical Details

* the floppy part is based on C#/.Net. Unfortunately these sources cannot be compiled, it can be only read. This is due to the use of  some proprietary library code (that I also created but it cannot be published here)
* the website runs on latest WordPress using Gutenberg block editor, custom scripts are implemented as child theme of the default Twenty Twenty One theme.

## Content Creation

* There is a CSV file containing the names of all guests (that is not part of this repository for data protection reasons)
* The index of nick names in alphabetical order is encoded using Base32 with `AB1CD2EF3GH4JK5LM6NP7QR8STUVWXYZ` as base digits into a unique ID for each guest.
* The [short.io](https://short.io) API is used to generate the personalized shortened link. Besides the unique ID it also contains some UTM query string parameters for campaign tracking using Google Analytics (GA is not activated currently)
* QRCoder is used to generate QR Codes in PNG format for all the links
* Figgle is used to create the ASCII arts 
* all placeholders in the template files under /res are replaced with the read or computed values for each guest and written to an output folder for each guest
* for each guest, the generated files get copied to the floppy after formatting it
* Initially it was planned to also automate the printing of the floppy labels, but this was too much of a pain, so each label text and qr code was copied manually into a word document 