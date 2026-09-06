//using System.Reflection;

//namespace TerribleSettingsAuditor.Core.Tests.Unit.Validation;

//public class ValidationTest : BaseTsaTest
//{
//    [Fact]
//    public async Task Validate_Pass()
//    {
//        try
//        {
//            // arrange
//            CancellationTokenSource cts = new();

//            List<Assembly> assemblies = new List<Assembly>();
//            assemblies.Add(typeof(TerribleSettingsAuditor.SampleApp.AssemblyMarker).Assembly);

//            // logger
//            var logger = GetLogger();

//            // tsa
//            var tsa = new TSA(logger);

//            // act
//            //var result = await tsa.ValidateAsync(app.ApplicationServices, assemblies.ToArray(), cts.Token);

//            // assert
//        }
//        catch (Exception ex)
//        {
//            throw;
//        }
//    }
//}
