using System;
using System.Collections.Generic;
using Awful.Models;

namespace Awful.Data
{
    public static class SAForums
    {
        public static List<ForumData> AllForums { get; private set; }

        static SAForums() 
        { 
          //  AllForums = GenerateForumList(); 
        }

        public static List<ForumData> GenerateForumList()
        {
           // var forumDB = 
            //    new Models.SAForumDataContext(Models.SAForumDataContext.DBConnectionString);

            List<Models.ForumData> forums = new List<Models.ForumData>()
            {
                new SAForum() { 
                    ForumName = "General Bullshit",
                    ID = 1, 
                },

                new SAForum() { 
                    ForumName = "FYAD",
                    ID = 26,
                    
                },

                new SAForum() { 
                    ForumName = "Front Page Discussion",
                    ID = 155,
                },

                new SAForum() { 
                    ForumName = "E/N Bullshit",
                    ID = 214,
                },
                
                new SAForum() { 
                    ForumName = "Games",
                    ID = 44,
                },

                new SAForum() { 
                    ForumName = "The MMO HMO",
                    ID = 145,
                },

                  new SAForum() { 
                    ForumName = "WoW: Goon Squad",
                    ID = 146,
                },

                  new SAForum() { 
                    ForumName = "The StarCraft II Zealot Zone",
                    ID = 250,
                },

                new SAForum() { 
                    ForumName = "Private Game Servers",
                    ID = 93,
                },

                new SAForum() { 
                    ForumName = "Traditional Games",
                    ID = 234,
                },

                new SAForum() { 
                    ForumName = "Let's Play!",
                    ID = 191,
                },

                new SAForum() { 
                    ForumName = "Inspect Your Gadgets",
                    ID = 192,
                },

                new SAForum() { 
                    ForumName = "The A/V Arena",
                    ID = 190,
                },

                new SAForum() { 
                    ForumName = "Ask / Tell",
                    ID = 158,
                },

                new SAForum() { 
                    ForumName = "Tourism & Travel",
                    ID = 211,
                },

                new SAForum() { 
                    ForumName = "Business, Finance, and Careers",
                    ID = 200,
                },

                new SAForum() { 
                    ForumName = "Debate & Discussion",
                    ID = 46,
                },

                new SAForum() { 
                    ForumName = "Science, Academics and Languages",
                    ID = 162,
                },

                new SAForum() { 
                    ForumName = "Serious Software / Software Crap",
                    ID = 22,
                },

                new SAForum() { 
                    ForumName = "Haus of Tech Support",
                    ID = 170,
                },

                new SAForum() { 
                    ForumName = "The Cavern of COBOL",
                    ID = 202,
                },

                new SAForum() { 
                    ForumName = "YOSPOS",
                    ID = 219,
                },

                new SAForum() { 
                    ForumName = "Sports Argument Stadium",
                    ID = 122,
                },

                  new SAForum() { 
                    ForumName = "The Football Funhouse",
                    ID = 181,
                },

                  new SAForum() { 
                    ForumName = "The Ray Parlour",
                    ID = 175,
                },

                  new SAForum() { 
                    ForumName = "Punchsport Pagoda",
                    ID = 177,
                },

                  new SAForum() { 
                    ForumName = "Poker in the Rear",
                    ID = 139,
                },

                  new SAForum() { 
                    ForumName = "Watch and Woot",
                    ID = 179,
                },

                  new SAForum() { 
                    ForumName = "The Goon Doctor",
                    ID = 183,
                },

                  new SAForum() { 
                    ForumName = "The Fitness Log Cabin",
                    ID = 244,
                },

                  new SAForum() { 
                    ForumName = "Goons With Spoons",
                    ID = 161,
                },

                  new SAForum() { 
                    ForumName = "Post Your Favorite",
                    ID = 167,
                },

                  new SAForum() { 
                    ForumName = "Automotive Insanity",
                    ID = 91,
                },

                  new SAForum() { 
                    ForumName = "Cycle Asylum",
                    ID = 236,
                },

                  new SAForum() { 
                    ForumName = "Pet Island",
                    ID = 124,
                },

                  new SAForum() { 
                    ForumName = "The Firing Range",
                    ID = 132,
                },

                  new SAForum() { 
                    ForumName = "The Crackhead Clubhouse",
                    ID = 90,
                },

                  new SAForum() { 
                    ForumName = "Goons in Platoons",
                    ID = 218,
                },

                  new SAForum() { 
                    ForumName = "Helldump Success Stories",
                    ID = 204,
                },

                  new SAForum() { 
                    ForumName = "YCS Goldmine",
                    ID = 229,
                },

                  new SAForum() { 
                    ForumName = "LF Goldmine",
                    ID = 222,
                },

                  new SAForum() { 
                    ForumName = "BYOB Goldmine",
                    ID = 176,
                },
                  new SAForum() { 
                    ForumName = "FYAD Goldmine",
                    ID = 115,
                },

                  new SAForum() { 
                    ForumName = "Comedy Goldmine",
                    ID = 21,
                },

                  new SAForum() { 
                    ForumName = "Creative Convention",
                    ID = 31,
                },

                  new SAForum() { 
                    ForumName = "DIY & Hobbies",
                    ID = 210,
                },

                 new SAForum() { 
                    ForumName = "The Dorkroom",
                    ID = 247,
                },

                 new SAForum() { 
                    ForumName = "Cinema Discusso",
                    ID = 151,
                },

                 new SAForum() { 
                    ForumName = "The Film Dump",
                    ID = 133,
                },

                 new SAForum() { 
                    ForumName = "The Book Barn",
                    ID = 182,
                },

                 new SAForum() { 
                    ForumName = "No Music Discussion",
                    ID = 150,
                },

                 new SAForum() { 
                    ForumName = "Musician's Lounge",
                    ID = 104,
                },

                 new SAForum() { 
                    ForumName = "The TV IV",
                    ID = 130,
                },

                 new SAForum() { 
                    ForumName = "Batman's Shameful Secret",
                    ID = 144,
                },

                 new SAForum() { 
                    ForumName = "ADTRW",
                    ID = 27,  
                },

                 new SAForum() { 
                    ForumName = "Entertainment, Weakly",
                    ID = 215,
                },

                 new SAForum() { 
                    ForumName = "Rapidly Going Deaf",
                    ID = 255,
                    
                },

                 new SAForum() { 
                    ForumName = "SA-Mart",
                    ID = 61,
                },

                 new SAForum() { 
                    ForumName = "Feedback & Discussion",
                    ID = 77,
                },

                 new SAForum() { 
                    ForumName = "Coupons & Deals",
                    ID = 85,
                    
                },

                 new SAForum() { 
                    ForumName = "Goon Meets",
                    ID = 43,
                    
                },

                 new SAForum() { 
                    ForumName = "LAN: Your City Sucks",
                    ID = 241,
                    
                },

                 new SAForum() { 
                    ForumName = "Questions, Comments, Suggestions?",
                    ID = 188,
                    
                },
            };

            return forums;
        }
    }
}
