using System;
using System.Collections.Generic;

namespace SecureAggregationAPI.Services
{
    public class SecretSharingService
    {
        public List<int> Split(int secret, int n)
        {
            List<int> shares = new List<int>();
            Random random = new Random();

            for (int i = 0; i < n - 1; i++)
            {
                int shareOne = random.Next(0, secret+1);
                int shareTwo = random.Next(0, secret-shareOne+1);
                int shareThree = secret-shareOne-shareTwo;
                shares.Add(shareOne);
                shares.Add(shareTwo);
                shares.Add(shareThree);
            }

            return shares;
        }
    }
}
