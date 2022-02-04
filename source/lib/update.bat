@echo off
setLocal EnableDelayedExpansion
set source="C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\"

echo Update libraries
echo Source %source:"=%

for /f "tokens=*" %%n in ('dir *.dll /b /a-d') do (
	set sourcefile="%source:"=%%%n"
	rem echo !sourcefile!
	if exist !sourcefile! (
		copy /Y !sourcefile! "%%n" >nul
		echo File %%n updated
	) else (
		echo File %%n doesn't exists in source folder
	)
)

echo|set/p="Press <ENTER> to continue.."&runas/u: "">NUL