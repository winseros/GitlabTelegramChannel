using Microsoft.Extensions.DependencyInjection;
using TGramWeb.Services.GitlabProcessService.RequestProcessors;

namespace TGramWeb.Services.GitlabProcessService
{
    internal static class GitlabProcessServiceModule
    {
        public static void AddGitlabProcessService(this IServiceCollection services)
        {
            services.AddSingleton<IGitlabProcessService, GitlabProcessServiceImpl>();
            //services.AddSingleton<IGitlabProcessor, MergeGitlabGitlabProcessor>();
            services.AddSingleton<IGitlabProcessor, PipelineFailureGitlabProcessor>();
        }
    }
}
