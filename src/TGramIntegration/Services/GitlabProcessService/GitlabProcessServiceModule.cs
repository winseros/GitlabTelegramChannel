using Microsoft.Extensions.DependencyInjection;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Comment;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.MergeRequest;
using TGramWeb.Services.GitlabProcessService.RequestProcessors.Pipeline;

namespace TGramWeb.Services.GitlabProcessService
{
    internal static class GitlabProcessServiceModule
    {
        public static void AddGitlabProcessService(this IServiceCollection services)
        {
            services.AddSingleton<IGitlabProcessService, GitlabProcessServiceImpl>();
            services.AddSingleton<IGitlabProcessor, MergeRequestGitlabProcessor>();
            services.AddSingleton<IMergeRequestMessageFormatter, MergeRequestMessageFormatter>();
            services.AddSingleton<IGitlabProcessor, PipelineFailureGitlabProcessor>();
            services.AddSingleton<IPipelineMessageFormatter, PipelineMessageFormatter>();
            services.AddSingleton<IGitlabProcessor, CommentSnippetGitlabProcessor>();
            services.AddSingleton<ICommentSnippetMessageFormatter, CommentSnippetMessageFormatter>();
            services.AddSingleton<IGitlabProcessor, CommentRequestGitlabProcessor>();
            services.AddSingleton<ICommentRequestMessageFormatter, CommentRequestMessageFormatter>();
        }
    }
}
