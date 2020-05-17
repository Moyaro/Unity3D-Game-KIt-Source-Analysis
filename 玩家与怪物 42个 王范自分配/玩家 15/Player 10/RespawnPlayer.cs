namespace Gamekit3D.GameCommands
{
    public class RespawnPlayer : GameCommandHandler
    {
        public Gamekit3D.PlayerController player;

        public override void PerformInteraction()//重写：类的继承时发生，在子类中重新定义父类中的方法，子类中的方法和父类的方法是一样的，即方法名，参数，返回值类型都相同。原文链接：https://blog.csdn.net/kj297296053/java/article/details/8213550
        {
            player.Respawn();//翻译：复位。      并在代码EllenRespawnEffect中引用
        }
    }
}
