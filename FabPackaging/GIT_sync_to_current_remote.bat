@echo off
@echo syncing local repo to current remote main repo and current submodule commits (not latest commits)

REM this only does submodules
REM git submodule update --recursive --init    

REM this does everything
git pull --recurse-submodules

@echo done!
pause

