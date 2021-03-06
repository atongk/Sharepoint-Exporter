﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace SpPrefetchIndexBuilder {
  
  public class SharepointExporterConfig {
    public string authScheme = "NTLM";
    public List<string> sites = new List<string>();
    public string outputDir;
    public bool customOutputDir;
    public int numThreads = 50;
    public bool excludeUsersAndGroups;
    public bool excludeGroupMembers;
    public bool excludeSubSites;
    public bool excludeLists;
    public bool excludeRoleDefinitions;
    public bool excludeRoleAssignments;
    public bool deleteExistingOutputDir;
    public bool excludeFiles;
    public int maxFiles = -1;
    public int maxListItems = -1;
    public int maxSites = -1;
    public int backoffRetries = 5;
    public int backoffInitialDelay = 1000;
    public JavaScriptSerializer serializer = new JavaScriptSerializer();
    public int maxFileSizeBytes = -1;
    public int fileCount;
    public int fileDownloadTimeoutSecs = 30;
    public string domain; 
    public string username;
    public string password;
    public bool isSharepointOnline;

    public SharepointExporterConfig(string[] args) {

      string spMaxFileSizeBytes = Environment.GetEnvironmentVariable("SP_MAX_FILE_SIZE_BYTES");
      if (spMaxFileSizeBytes != null) {
        maxFileSizeBytes = int.Parse(spMaxFileSizeBytes);
      }
      string spNumThreads = Environment.GetEnvironmentVariable("SP_NUM_THREADS");
      if (spNumThreads != null) {
        numThreads = int.Parse(spNumThreads);
      }
      serializer.MaxJsonLength = 1677721600;

      bool help = false;

      password = Environment.GetEnvironmentVariable("SP_PWD");
      outputDir = Directory.GetCurrentDirectory();
      customOutputDir = false;
      string sitesFilePath = null;

      foreach (string arg in args) {
        if (arg.Equals("--help") || arg.Equals("-help") || arg.Equals("/help")) {
          help = true;
          break;
        }
        if (arg.StartsWith("--sitesFile=", StringComparison.CurrentCulture)) {
          sitesFilePath = arg.Split(new Char[] { '=' })[1];
        } else if (arg.StartsWith("--sharepointUrl=", StringComparison.CurrentCulture)) {
          sites.Add(Util.addSlashToUrlIfNeeded(arg.Split(new Char[] { '=' })[1]));
        } else if (arg.StartsWith("--outputDir=", StringComparison.CurrentCulture)) {
          outputDir = arg.Split(new Char[] { '=' })[1];
          customOutputDir = true;
        } else if (arg.StartsWith("--deleteExistingOutputDir=", StringComparison.CurrentCulture)) {
          deleteExistingOutputDir = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--authScheme=", StringComparison.CurrentCulture)) {
          authScheme = arg.Split(new Char[] { '=' })[1];
        } else if (arg.StartsWith("--domain=", StringComparison.CurrentCulture)) {
          domain = arg.Split(new Char[] { '=' })[1];
        } else if (arg.StartsWith("--username=", StringComparison.CurrentCulture)) {
          username = arg.Split(new Char[] { '=' })[1];
        } else if (arg.StartsWith("--password=", StringComparison.CurrentCulture)) {
          password = arg.Split(new Char[] { '=' })[1];
        } else if (arg.StartsWith("--numThreads=", StringComparison.CurrentCulture)) {
          numThreads = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--backoffInitialDelay=", StringComparison.CurrentCulture)) {
          backoffInitialDelay = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--backoffRetries=", StringComparison.CurrentCulture)) {
          backoffRetries = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--maxFileSizeBytes=", StringComparison.CurrentCulture)) {
          maxFileSizeBytes = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--maxFiles=", StringComparison.CurrentCulture)) {
          maxFiles = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--maxListItems=", StringComparison.CurrentCulture)) {
          maxListItems = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--maxSites=", StringComparison.CurrentCulture)) {
          maxSites = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--fileDownloadTimeoutSecs=", StringComparison.CurrentCulture)) {
          fileDownloadTimeoutSecs = int.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeUsersAndGroups=", StringComparison.CurrentCulture)) {
          excludeUsersAndGroups = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeGroupMembers=", StringComparison.CurrentCulture)) {
          excludeGroupMembers = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeSubSites=", StringComparison.CurrentCulture)) {
          excludeSubSites = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeLists=", StringComparison.CurrentCulture)) {
          excludeLists = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeRoleAssignments=", StringComparison.CurrentCulture)) {
          excludeRoleAssignments = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeRoleDefinitions=", StringComparison.CurrentCulture)) {
          excludeRoleDefinitions = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else if (arg.StartsWith("--excludeFiles=", StringComparison.CurrentCulture)) {
          excludeFiles = Boolean.Parse(arg.Split(new Char[] { '=' })[1]);
        } else {
          Console.WriteLine("ERROR - Unrecognized argument {0}.", arg);
          help = true;
        }
      }
      if (sitesFilePath != null) {
        FileInfo sitesFile = new FileInfo(sitesFilePath);
        if (!sitesFile.Exists) {
          Console.WriteLine("Error - sites file {0} doesn't exist", sitesFilePath);
          Environment.Exit(1);
        }
        if (sitesFile != null && sitesFile.Exists) {
          foreach (string nextSite in File.ReadLines(sitesFile.FullName)) {
            string nextSiteWithSlashAddedIfNeeded = Util.addSlashToUrlIfNeeded(nextSite);
            if (!sites.Contains(nextSiteWithSlashAddedIfNeeded)) {
              sites.Add(nextSiteWithSlashAddedIfNeeded);
            }
          }
        }
      }

      if (sites.Count <= 0) {
        Console.WriteLine("ERROR - Must specify --sharepointUrl argument or a --sitesFile argument to specify what sharepoint sites to fetch.");
        help = true;
      }

      if (help) {
        Console.WriteLine(new StringBuilder().AppendLine("USAGE: SpPrefetchIndexBuilder.exe")
                          .AppendLine("    --sharepointUrl=[The sharepoint url. I.e. http://oursharepoint]   (*required)")
                          .AppendLine("    --incrementalFile=[optional - path to incremental file created during a previous run. if specified, will only fetch incremental changes based on this file.]")
                          .AppendLine("    --sitesFile=[optional - path to sites file. this is a list]")
                          .AppendLine("    --outputDir=[optional - where to save the output. default will use this directory.]")
                          .AppendLine("    --domain=[optional - netbios domain of the user to crawl as]")
                          .AppendLine("    --username=[optional - specify a username to crawl as. must specify domain if using this]")
                          .AppendLine("    --password=[password (not recommended, do not specify to be prompted or use SP_PWD environment variable)]")
                          .AppendLine("    --numThreads=[optional number of threads to use while fetching. Default 50]")
                          .AppendLine("    --backoffRetries=[optional number of times to retry after a csom failure. Default 5]")
                          .AppendLine("    --backoffInitialDelay=[optional number of milliseconds to set as the initial backoff delay. Default 1000]")
                          .AppendLine("    --numThreads=[optional number of threads to use while fetching. Default 50]")
                          .AppendLine("    --excludeUsersAndGroups=[exclude users and groups from the top level site collections. default false]")
                          .AppendLine("    --excludeGroupMembers=[exclude group members from the UsersAndGroups section. default false]")
                          .AppendLine("    --excludeRoleDefinitions=[if true will not store obtain role definition metadata from the top level site collections. default false] ")
                          .AppendLine("    --excludeSubSites=[only output the top level sites, do not descend into sub-sites. default false]")
                          .AppendLine("    --excludeLists=[exclude lists from the results. default false]")
                          .AppendLine("    --excludeFiles=[Do not download the files from the results] ")
                          .AppendLine("    --excludeRoleAssignments=[if true will not store obtain role assignment metadata. default false] ")
                          .AppendLine("    --maxFileSizeBytes=[optional maximum file size. Must be > 0. Default unlimited]")
                          .AppendLine("    --maxFiles=[if > 0 will only download this many files. default -1]")
                          .AppendLine("    --maxListItems=[if > 0 will only fetch this many list items. default -1]")
                          .AppendLine("    --maxSites=[if > 0 will only fetch this many sites. default -1]")
                         );
        Environment.Exit(0);
      }
      Regex r = new Regex(@"(?<Protocol>\w+):\/\/.+\.sharepoint\.com.*");
      isSharepointOnline = r.Match(sites[0]).Success;

    }
  }
}
