﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension.Client;
using Grpc.Extension.Client.LoadBalancer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Grpc.Client
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddGrpcClient(options =>
			{
				options.AddLoadBalancer(ILoadBalancer.WeightedPolling);
				options.AddConsul(client => { client.Address = new Uri("http://192.168.1.142:8500"); });
				options.AddServiceCredentials("grpc-server", ChannelCredentials.Insecure);
				options.ChannelStatusCheckInterval = TimeSpan.FromSeconds(15);
				options.AddClient<Hello.HelloClient>();
				options.AddCircuitBreaker(conf => { conf.InvokeTimeout = TimeSpan.FromSeconds(1); });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseGrpcClient();
			app.UseMvc();
		}
	}
}
