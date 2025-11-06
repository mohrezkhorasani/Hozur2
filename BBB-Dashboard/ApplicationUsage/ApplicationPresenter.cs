using Application.Interface.IUsecase;

namespace SafaretoWeb.ApplicationUsage
{
    public class GenericPresenter<T> : IOutPutPort<T>
    {
        public T Response { get; private set; }
        public void Handle(T response) => Response = response;
    }

}
