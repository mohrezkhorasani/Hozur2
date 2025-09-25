using System.Threading.Tasks;

namespace Application.Interface.IUsecase
{

    public interface IUseCaseRequestHandler<in TUseCaseRequest, out TUseCaseResponse> where TUseCaseRequest : IUseCaseRequest<TUseCaseResponse>
    {
        Task handleAsync(TUseCaseRequest message, IOutPutPort<TUseCaseResponse> outPutPort);
    }

    public interface IUseCaseRequest<out TUseCaseResponse> { }

    public interface IOutPutPort<in TUseCaseResponse>
    {
        void Handle(TUseCaseResponse resp);
    
    }
}