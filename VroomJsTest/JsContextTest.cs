namespace VroomJsTest
{
  using NUnit.Framework;

  using VroomJs;

  [TestFixture]
  public class JsContextTest
  {
    [OneTimeSetUp]
    public void Init()
    {
      AssemblyLoader.EnsureLoaded();
    }

    [Test]
    public void AddTest()
    {
      using (var engine = new JsEngine())
      {
        using (var context = engine.CreateContext())
        {
          var x = (double)context.Execute("3.14159+2.71828");
          Assert.AreEqual(x, 5.85987);
        }
      }
    }

    [Test]
    public void tellme_Test()
    {
      using (JsEngine js = new JsEngine(4, 32))
      {
        using (JsContext context = js.CreateContext())
        {
          context.Execute("var x = {'answer':42, 'tellme':function (x) { return x+' '+this.answer; }}");
          dynamic x = context.GetVariable("x");
          var result = x.tellme("What is the answer to ...?");
          Assert.AreEqual(result, "What is the answer to...? 42");
        }
      }
    }

    [Test]
    public void AccessPropertiesAndCallMethodsTest()
    {
      using (JsEngine js = new JsEngine(4, 32))
      {
        using (JsContext context = js.CreateContext())
        {
          context.SetVariable("m", new Test());
          context.Execute("m.Value = 42");
          var result = context.Execute("m.PrintValue('And the answer is (again!):')");
          Assert.AreEqual(result, "And the answer is (again!): 42");
        }
      }
    }

    private class Test
    {
      public int Value { get; set; }

      public string PrintValue(string msg)
      {
        return msg + " " + Value;
      }
    }
  }
}
