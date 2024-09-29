using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Stores a complete list of all available modules
    /// </summary>
    [CreateAssetMenu(fileName = "ModuleManager", menuName = "Paywall/Modules/ModuleManager")]
    public class ModuleManager : ScriptableList<ScriptableModule>
    {

    }
}
