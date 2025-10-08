::  Deploy FeedReader to Prod
net use X: /DELETE
net use X: \\Katla\cdrive\apps


del x:\FeedReader\backup\*.* /q
xcopy x:\FeedReader\*.* x:\FeedReader\backup\

xcopy *.dll x:\FeedReader /y
xcopy *.pdb x:\FeedReader /y
xcopy *.exe x:\FeedReader /y
xcopy *.bat x:\FeedReader /y

xcopy FeedReader*.json x:\FeedReader /y


pause