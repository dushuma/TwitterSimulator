using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using NDesk.Options;
using NLog;

namespace TwitterSimulator
{
  public class Program
  {
    private static Logger _Logger = LogManager.GetCurrentClassLogger();
    private static Encoding Detected;

    public static void Main(string[] args)
    {
      //main inouts
      string userFile = null;
      string tweetsFile = null;
      bool showHelp = false;

      //option set for user input args
      var options = new OptionSet
            {
                {
                    "u|userfile=",
                    "Location of user data",
                    x =>
                    {
                        _Logger.Info("Using user file = " + x);
                        userFile = x;
                    }
                },
                {
                    "t|tweetfile=",
                    "Location of tweets data",
                    x =>
                    {
                        _Logger.Info("Using tweet file = " + x);
                        tweetsFile = x;
                    }
                },
                {
                    "h|help",
                    "Show help options",
                    x => showHelp =  x != null
                }
            };

      List<string> extra;//erronous args
      try
      {
        extra = options.Parse(args);
      }
      catch (OptionException e)
      {
        _Logger.Error("Error parsing input flags. Please refer to specification below:");
        _Logger.Error(e.Message);
        _Logger.Error("Try {0} --help' for more information.", System.AppDomain.CurrentDomain.FriendlyName);

        // Show error and help information
        ShowHelp(options);
        return;
      }

      if(showHelp)
      {
        ShowHelp(options);
        return;
      }

      // Ensure that all flags have been populated
      if (String.IsNullOrEmpty(userFile) || String.IsNullOrEmpty(tweetsFile))
      {
        _Logger.Error("All input flags must be populated. Please refer to specification below:");
        _Logger.Error("Try {0} --help for more information.", AppDomain.CurrentDomain.FriendlyName);
        _Logger.Warn("Please ensure none of the file path inputs end in \\");

        // Show erro and help information
        ShowHelp(options);
        return;
      }

      //get inout file, check if admissible
      string user_File = testFile(userFile);
      string tweet_File = testFile(tweetsFile);

      checkFileFormat(userFile, ".* follows .*");// cant affix variable lenght csvs here so just basic check, serious err on csv here
      checkFileFormat(tweetsFile, ".*> .*");// cant affix variable lenght csvs here, no proper error separation

      Console.WriteLine("Starting Simulation...");
      simulateTweets(userFile,tweetsFile);

      Console.WriteLine("Press Enter key to exit simulation...");
      Console.ReadLine();
      return;
    }








    /// <summary>
    /// Shows the help message for command line arguments.
    /// </summary>
    /// <param name="p"></param>
    static void ShowHelp(OptionSet p)
    {

      _Logger.Info("Usage: {0} [OPTIONS]+ message", System.AppDomain.CurrentDomain.FriendlyName);
      _Logger.Info("Options:");
      var s = new StringWriter();
      p.WriteOptionDescriptions(s);
      p.WriteOptionDescriptions(Console.Out);
      s.Flush();
      _Logger.Info(s.ToString());
    }



    /// <summary>
    /// test existence of input files
    /// </summary>
    /// <param name="fileLoc">file location.</param>
    private static string testFile(string fileLoc)
    {
      // test file or file locations
      try
      {
        if (!File.Exists(fileLoc))
          throw new FileNotFoundException();
        else
          return fileLoc;
      }
      catch (FileNotFoundException theException)
      {
        _Logger.Error("Unable to get file at path <{0}>. Check that path and file do exist. Make sure you're not ending your input path with a '\\' and that you have escaped/quoted all paths. Exception was: {1}\r\n{2}", fileLoc, theException.Message, theException.StackTrace);
        throw;
      }

    }












    /// <summary>
    /// test format of file
    /// </summary>
    /// <param name="fileLoc">file location.</param>
    /// <param name="pattern">pattern for regular expression.</param>
    private static void checkFileFormat(string fileLoc, string pattern)
    {
      // test file or file locations
      try
      {
        StreamReader ut = new StreamReader(fileLoc, Encoding.ASCII);//assuming asciii here
        string[] delims = { "\r\n" };
        string[] inData = ut.ReadToEnd().Split(delims, StringSplitOptions.None);
        foreach (string s in inData)
        {
          if (!s.Equals(""))
          {
            if (!Regex.IsMatch(s, pattern))
            {
              throw new NotImplementedException("Failed recognising input file");//no time to create UD exception here
            }
          }
        }
        return;

      }
      catch (NotImplementedException theException)
      {
        _Logger.Error("File data does not conform to expected input format. Exception was: {1}\r\n{2}", fileLoc, theException.Message, theException.StackTrace);
        Console.WriteLine(theException);
        throw;
      }

    }













        /// <summary>
        /// Simulates "twitter", with user, follower and twet logic embedded
        /// </summary>
        /// <param name="uFile">user file.</param>
        /// <param name="tFile">tweets file</param>
    public static void simulateTweets(String uFile, String tFile)
    {

      //readfiles
      StreamReader ut = new StreamReader(uFile, Encoding.ASCII);//assumed input is already ASCII 7-bit so preserve 
      StreamReader tt = new StreamReader(tFile, Encoding.ASCII);//assumed input is already ASCII 7-bit so preserve
      StreamReader ft = new StreamReader(uFile, Encoding.ASCII);//assumed input is already ASCII 7-bit so preserve 
      Detected = ut.CurrentEncoding;//assumed input is already ASCII 7-bit so preserve 

      List<string> users = new List<string>();
      List<List<string>> followers = new List<List<string>>(); ;//followers are user+followers plus union on followers

      //delimiters on reg read of inout files
      string[] delimsU = { " follows ", ", ", "\r\n" };
      string[] delimsFoll = { " follows ", ", "};
      string[] delimsT1 = { "\r\n" };
      string[] delimsT2 = { "> " };


      string[] uData = ut.ReadToEnd().Split(delimsU, StringSplitOptions.None);
      string[] tData = tt.ReadToEnd().Split(delimsT1, StringSplitOptions.None);

      //unique users
      foreach (string s in uData)
      {
        string ss = s.Trim();
        if (!ss.Equals(""))
        {
          if (users.IndexOf(ss) < 0)
          {
            users.Add(ss);
            //Console.WriteLine(ss);
            //Console.WriteLine(users.Contains(ss));
          }
        }
      }



      //sort users alphabetically
      users.Sort();

      //unique user followers relationship
      string temp1;
      while ((temp1 = ft.ReadLine()) != null)
      {
        string[] temp2 = temp1.Split(delimsFoll, StringSplitOptions.None);
        List<string> temp = new List<string>();
        for (int i = 0; i < temp2.Count(); i++)
        {
          temp.Add(temp2[i]);
        }
        bool testF = false;
        int locInt = -1;
        for (int i = 0; i < followers.Count(); i++)
        {
          if (temp[0].Equals(followers[i][0]))
          {
            locInt = i;
            testF = true;
            break;
          }
        }

        if (testF)
        {
          for (int i = 1; i < temp.Count(); i++)
          {
            if (!followers[locInt].Contains(temp[i]))//NBBBBBusers can only add followers and not remove
            {
              followers[locInt].Add(temp[i]);

            }
          }
        }
        else
        {
          followers.Add(temp);
        }
      }

      //tweets in 2D, indexed by user
      List<List<string>> tweets = new List<List<string>>();
      foreach (string t in tData)
      {
        if (!t.Equals(""))
        {
          List<string> temp = new List<string>();
          string[] ttt = t.Split(delimsT2, StringSplitOptions.None);
          temp.Add(ttt[0]);
          temp.Add(ttt[1]);
          tweets.Add(temp);
        }
      }

      //for users, given followers, retweet chronologically
      int locF = -1;
      foreach (string u in users)
      {
        Console.WriteLine("{0} \r\n", u, Detected);
        for (int i = 0; i <= followers.Count - 1; i++)
        {
          if (followers[i][0].Equals(u))
          {
            locF = i;
          }
        }
        for (int i = 0; i <= tweets.Count - 1; i++)//preserves the order of tweets
        {
          if (locF != -1)
          {
            for (int j = 0; j <= followers[locF].Count - 1; j++)
            {
              if (followers[locF][j].Equals(tweets[i][0]))
              {
                Console.WriteLine("\t @{0}: {1} \r\n", followers[locF][j], tweets[i][1],Detected);
              }
            }
          }
        }
        locF = -1;
      }

      return;
    }
  }
}
