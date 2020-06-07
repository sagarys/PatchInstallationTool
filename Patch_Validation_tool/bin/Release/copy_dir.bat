echo "copying....."
ROBOCOPY %1 %2 /S /R:1 /W:1 /LOG:%3﻿
echo "copied !!!!!!!!"

IF %ERRORLEVEL% NEQ 1 ( 
	IF %ERRORLEVEL% NEQ 0 ( 
	   echo %ERRORLEVEL%
	   echo "Error While Copying file" 
	   EXIT 1
	)
	ELSE (
		echo %ERRORLEVEL%
		echo "File Already Exists No Copying Done" 
	)
)

EXIT 0