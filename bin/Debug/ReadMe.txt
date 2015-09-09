Created by Mash Makananise

9-9-2015

32 bit app


****************************************************
Usage:

Open cmd at folder where TwitterSimulator.exe is located, give user.txt and tweet.txt files plus location, application will simulated tweets.


<.loc of exe>\TwitterSimulator.exe -u C:\(...location...)\user.txt -t C:\(...location...)\tweet.txt

help directions can be envoked as such:
<.loc of exe>\TwitterSimulator.exe -h

****************************************************
Tests

#dirty user file
<.loc of exe>\TwitterSimulator.exe -u C:\(...location...)\userDirty.txt -t C:\(...location...)\tweet.txt



#dirty tweet file
<.loc of exe>\TwitterSimulator.exe -u C:\(...location...)\user.txt -t C:\(...location...)\tweetDirty.txt

#unexpected inputs
<.loc of exe>\TwitterSimulator.exe -u C:\(...location...)\user.txt -t C:\(...location...)\tweet.txt -r -y