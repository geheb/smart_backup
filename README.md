# SmartBackup [![Build status](https://ci.appveyor.com/api/projects/status/5cn0gm9at2quej7l/branch/master?svg=true)](https://ci.appveyor.com/project/gethomast/smart-backup/branch/master)
Simple full and incremental backup based on 7-Zip compression with encryption.

<p align="center">
	<img src="https://raw.github.com/geheb/smart_backup/master/logo.svg?sanitize=true">
</p>

## Prerequisites
* Windows 10

## Install
Get the latest [Package](https://github.com/geheb/smart_backup/releases/latest)

## Usage
SmartBackup has a command line interface only.

Make a backup of the folder C:\foo and save it in the c:\backup folder with password bar, keep a maximum of 30 backup sets:

	SmartBackup backup -s "C:\foo" -t "C:\backup" -p "bar" -m 30

Make a backup of the folder C:\foo and C:\bar, save it in the c:\backup folder with password baz, keep a maximum of 30 backup sets:

	SmartBackup backup -s "c:\foo" -s "c:\bar" -t "C:\backup" -p "baz" -m 30

## License
[MIT](https://github.com/geheb/smart_backup/blob/master/LICENSE)
