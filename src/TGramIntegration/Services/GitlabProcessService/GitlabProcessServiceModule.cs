using Microsoft.Extensions.DependencyInjection;
using TGramIntegration.Services.GitlabProcessService.RequestProcessors;

namespace TGramIntegration.Services.GitlabProcessService
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
