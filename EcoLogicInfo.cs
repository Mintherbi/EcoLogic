using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace EcoLogic
{
  public class EcoLogicInfo : GH_AssemblyInfo
  {
    public override string Name => "EcoLogic Info";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => null;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "";

    public override Guid Id => new Guid("4D5E7D51-9BD9-4443-8AA9-B6B0868CC35C");

    //Return a string identifying you or your company.
    public override string AuthorName => "";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "";

    //Return a string representing the version.  This returns the same version as the assembly.
    public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
  }
}
