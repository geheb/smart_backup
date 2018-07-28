# smart_backup [![Build status](https://ci.appveyor.com/api/projects/status/5cn0gm9at2quej7l/branch/master?svg=true)](https://ci.appveyor.com/project/gethomast/smart-backup/branch/master)
Simple incremental backup based on 7-Zip compression and encryption.

<p align="center">
	<img src="https://raw.github.com/geheb/smart_backup/master/logo.svg?sanitize=true">
</p>

## Prerequisites
* Windows 10
* [.NET Framework 4.7](https://www.microsoft.com/net/download/framework)

## Windows Computer
Get the latest [Package](https://github.com/geheb/smart_backup/releases/latest)

## Usage
smart_backup has a command line interface only.

Make a backup of the folder C:\foo and save it in the c:\backup folder with password bar, keep a maximum of 30 backup sets:

	smart_backup backup -f "C:\foo" -t "C:\backup" -p "bar" -m 30

Make a backup of the folder C:\foo and C:\bar, save it in the c:\backup folder with password baz, keep a maximum of 30 backup sets:

	smart_backup backup -f "c:\foo" "c:\bar" -t "C:\backup" -p "baz" -m 30

## License
[MIT](https://github.com/geheb/smart_backup/blob/master/LICENSE)
