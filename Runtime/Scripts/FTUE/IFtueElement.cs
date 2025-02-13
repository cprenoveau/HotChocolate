using System.Threading;
using System.Threading.Tasks;

namespace HotChocolate.FTUE
{
    public interface IFtueElement
    {
        /// <summary>
        /// This will be called on all currently registered ftue elements (see FtueEventListener.RegisterElement)
        /// One of them should return true. (else the tutorial step does nothing)
        /// If many would return true, only the first one found will be used
        /// </summary>
        bool ShouldActivateFtue(IFtueStep ftueStep);

        /// <summary>
        /// The tutorial will await this task on the ftue element returned by ShouldActivate
        /// </summary>
        Task ActivateFtue(IFtueStep ftueStep, CancellationToken ct);
    }
}
