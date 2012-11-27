using System;
using Awful.Models;
using System.Collections.Generic;

namespace Awful.SampleData
{
    public class SamplePostData : SAPost
    {
        readonly string icon = "/icons/awfultiny.png";
        readonly Uri iconUri = new Uri("/icons/awfultiny.png", UriKind.RelativeOrAbsolute);
        readonly string author = "SA Poster";
        
        public SamplePostData() : base()
        {
            PostIconUri = icon;
            PostAuthor = author;
            PostDate = DateTime.Now;
            ShowPostIcon = true;
        }

        public static IList<PostData> GenerateSamplePosts(int size)
        {
            List<PostData> list = new List<PostData>();
            for (int i = 0; i < size; i++)
                list.Add(new SamplePostData() { PostIndex = i });

            return list;
        }
    }
}
