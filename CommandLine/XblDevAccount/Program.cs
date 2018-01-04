﻿using System.Net;

namespace XblDevAccount
{
    using CommandLine;
    using CommandLine.Text;
    using Microsoft.Xbox.Services.Tool;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class Program
    {

        private enum AccountSourceOption
        {
            XDP = DevAccountSource.XboxDeveloperPortal,
            WindowsDeveloperPortal = DevAccountSource.WindowsDevCenter,
        }


        [Verb("signin", HelpText = "Sign in a xbox live develoepr account.")]
        private class SignInOptions
        {
            [Option('s', "accountSource", Required = true,
                HelpText =
                    "The account source where the developer account was registered. Accept 'WindowsDevCenter' or 'XDP'")]
            public AccountSourceOption AccountSource { get; set; }

            [Option('u', "userName", Required = true,
                HelpText = "The user name of the account.")]
            public string UserName { get; set; }

            [Usage]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Sign In a XBOX Live developer account",
                        new SignInOptions
                        {
                            AccountSource = AccountSourceOption.WindowsDeveloperPortal,
                            UserName = "xxxx@xxx.com"
                        });
                }
            }
        }

        [Verb("signout", HelpText = "Sign out the signed in develoepr account.")]
        private class SignOutOptions
        {
            [Usage]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Sign out the Xbox Live developer account", new SignOutOptions());
                }
            }
        }

        [Verb("show", HelpText = "Display current signed in user.")]
        private class ShowOptions
        {
            [Usage]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Display current signed in user.", new ShowOptions());
                }
            }
        }

        static async Task<int> Main(string[] args)
        {
            try
            {
                string invokedVerb = string.Empty;
                SignInOptions signInOptions = null;

                // Only assign the option and verb here, as the commandlineParser doesn't support async callback yet.
                var result = Parser.Default.ParseArguments<SignInOptions, SignOutOptions, ShowOptions>(args)
                    .WithParsed<SignInOptions>(options =>
                    {
                        invokedVerb = "signin";
                        signInOptions = options;
                    })
                    .WithParsed<SignOutOptions>(options => OnSignOut())
                    .WithParsed<ShowOptions>(options => OnShow());


                if (result.Tag == ParserResultType.NotParsed)
                {
                    return -1;
                }

                if (invokedVerb == "signin" && signInOptions != null)
                {
                    await OnSignIn(signInOptions);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: unexpected error found.");
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        private static async Task OnSignIn(SignInOptions signInOptions)
        {
            try
            {
                Auth.SetAuthInfo(signInOptions.UserName, (DevAccountSource) signInOptions.AccountSource);
                var devAccount = await Auth.SignIn();
                Console.WriteLine($"Developer account {devAccount.Name} has successfully signed in. ");
                DisplayDevAccount(devAccount, "\t");
            }
            catch (XboxLiveException ex)
            {
                Console.WriteLine("Error: signin failed");
                if (ex.Response != null && ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Unable to authorize this account with XboxLive service, please check your account.");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void DisplayDevAccount(DevAccount devAccount, string indent)
        {
            Console.WriteLine($"{indent}ID : {devAccount.Id}");
            Console.WriteLine($"{indent}Publisher ID : {devAccount.AccountId}");
            Console.WriteLine($"{indent}AccountType : {devAccount.AccountType}");
            Console.WriteLine($"{indent}AccountMoniker : {devAccount.AccountMoniker}");
            Console.WriteLine($"{indent}AccountSource : {devAccount.AccountSource}");
        }

        private static void OnSignOut()
        {
            DevAccount account = Auth.LoadLastSignedInUser();
            if (account != null)
            {
                Auth.SignOut();
                Console.WriteLine($"Developer account {account.Name} from { account.AccountSource} has successfully signed out. ");
            }
            else
            {
                Console.WriteLine($"No signed in account found.");
            }
        }

        private static void OnShow()
        {
            DevAccount account = Auth.LoadLastSignedInUser();
            if (account != null)
            {
                Console.WriteLine($"Developer account {account.Name} from { account.AccountSource} is currently signed in. ");
                DisplayDevAccount(account, "\t");
            }
            else
            {
                Console.WriteLine($"No signed in account found.");
            }
        }
    }
}