echo "copying....."
ROBOCOPY %1 %2 %3  /R:1 /W:1 /IS /IT /LOG:%4 
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

rem pause 
EXIT 0