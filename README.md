
# DosAttackMiddleware
Asp.net Core DosAttackMiddleware

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseDosAttackMiddleware();
    }
# Settings
       private const int BannedRequests = 10;
       private const int ReductionInterval = 1000; // 1 second
       private const int ReleaseInterval = 1 * 60 * 1000; // 1 minutes
