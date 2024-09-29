namespace Paywall
{

    public class RunnerStoreMenuManager : StoreMenuManager
    {

        protected override void Update()
        {
            if (PaywallProgressManager.HasInstance && (MoneyCounter != null))
            {
                MoneyCounter.text = PaywallProgressManager.Instance.Trinkets.ToString();
            }
        }

    }

}
