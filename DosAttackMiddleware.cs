using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Timers;
namespace YourNameSpace.Middlewares
{
    public class DosAttackMiddleware
    {
        public class IpadressModel
        {
            public string IpAddress { get; set; }
            public short Counter { get; set; }
        }
        #region Private fields
        private static readonly List<IpadressModel> IpAdresses = new List<IpadressModel>();
        private static readonly Stack<string> Banned = new Stack<string>();
        private static Timer _timer = CreateTimer();
        private static Timer _bannedTimer = CreateBanningTimer();

        private const int BannedRequests = 10;
        private const int ReductionInterval = 1000; // 1 second
        private const int ReleaseInterval = 1 * 60 * 1000; // 1 minutes
        #endregion

        #region Middleware Members
        private RequestDelegate _next;
        public DosAttackMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            // Do something with context near the beginning of request processing.
            string ip = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            CheckIpAddress(ip);

            if (Banned.Contains(ip))
            {
                context.Response.StatusCode = 403;
              
            }
            else
            {
                await _next.Invoke(context);
            }
        }
        #endregion

        private static void CheckIpAddress(string ip)
        {
            if (!IpAdresses.Any(x => x.IpAddress.Equals(ip)))
            {
                IpAdresses.Add(new IpadressModel() {IpAddress = ip,Counter = 1});
                return;
            }

            var ipaddres = IpAdresses.FirstOrDefault(x => x.IpAddress.Equals(ip));
            if (ipaddres.Counter == BannedRequests)
            {
                Banned.Push(ip);
                IpAdresses.Remove(ipaddres);
            }
            else
            {
                ipaddres.Counter++;
            }
        }

        #region Timers
        /// <summary>
        /// Creates the timer that substract a request
        /// from the _IpAddress dictionary.
        /// </summary>
        private static Timer CreateTimer()
        {
            Timer timer = GetTimer(ReductionInterval);
            timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            return timer;
        }
        /// <summary>
        /// Creates the timer that removes 1 banned IP address
        /// everytime the timer is elapsed.
        /// </summary>
        /// <returns></returns>
        private static Timer CreateBanningTimer()
        {
            Timer timer = GetTimer(ReleaseInterval);
            timer.Elapsed += delegate
            {
               if (Banned.Count == 0) return;
                Banned?.Pop();
            };
            return timer;
        }
        /// <summary>
        /// Creates a simple timer instance and starts it.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        private static Timer GetTimer(int interval)
        {
            Timer timer = new Timer();
            timer.Interval = interval;
            timer.Start();

            return timer;
        }
        /// <summary>
        /// Substracts a request from each IP address in the collection.
        /// </summary>
        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < IpAdresses.Count; i++)
            {
                IpAdresses[i].Counter--;
                if (IpAdresses[i].Counter == 0)
                    IpAdresses.Remove(IpAdresses[i]);
            }
        }
        #endregion
    }
    public static class DosAttackMiddlewareExtensions
    {
        public static IApplicationBuilder UseDosAttackMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DosAttackMiddleware>();
        }
    }
}
