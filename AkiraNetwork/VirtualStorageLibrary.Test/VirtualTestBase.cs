using System.Diagnostics;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualTestBase
    {
        private static TextWriter? originalOutput;
        private static StreamWriter? logWriter;
        private static string? logDirectory;
        private static string? logFilePath;
        public TestContext? TestContext { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // ログディレクトリを設定
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestLog");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 日時を含むログファイル名を設定
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            logFilePath = Path.Combine(logDirectory, $"TestLog-{timestamp}.txt");

            // 元の出力を保存
            originalOutput = Console.Out;

            // ログファイルを作成
            logWriter = new(logFilePath)
            {
                AutoFlush = true
            };

            // 標準出力と標準エラー出力をリダイレクト
            Console.SetOut(logWriter);
            Console.SetError(logWriter);

            // Debugの出力もリダイレクト
            Trace.Listeners.Add(new TextWriterTraceListener(logWriter));
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // 元の出力がnullでない場合のみ戻す
            if (originalOutput != null)
            {
                Console.SetOut(originalOutput);
                Console.SetError(originalOutput);
            }

            // Debugのリスナーをクリア
            Trace.Listeners.Clear();

            // ログファイルがnullでない場合のみ閉じる
            logWriter?.Close();
        }

        [TestInitialize]
        public virtual void TestInitialize()
        {
            // テストメソッド名をデバッグ出力
            Debug.WriteLine($"\n\n{TestContext?.TestName}() :");
        }
    }
}
