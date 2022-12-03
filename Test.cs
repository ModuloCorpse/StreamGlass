using StreamChemistry;
using StreamChemistry.Base;
using System;

namespace StreamGlass
{
    public class Test
    {
        private bool TestMoleculeCase()
        {
            Laboratory laboratory = new();
            laboratory.AddNucleus(new BaseEntryNucleus()); //Entry
            laboratory.AddNucleus(new EqualsNucleus()); //==
            laboratory.AddNucleus(new IfNucleus()); //If

            Molecule molecule = laboratory.NewMolecule<string>();

            Atom? entryPoint = molecule.NewAtom("Entry");
            if (entryPoint == null) return false;
            Atom? ifCondition = molecule.NewAtom("If");
            if (ifCondition == null) return false;
            Atom? conditionEvaluator = molecule.NewAtom("==");
            if (conditionEvaluator == null) return false;

            Atom valueA = molecule.NewValueAtom(42);
            Atom valueB = molecule.NewValueAtom(69);
            Atom valueTrue = molecule.NewValueAtom("Hello");
            Atom valueFalse = molecule.NewValueAtom("World");
            Atom returnTrue = molecule.NewReturnAtom();
            Atom returnFalse = molecule.NewReturnAtom();

            entryPoint.Bond("", ifCondition);
            ifCondition.Bond("True", returnTrue);
            ifCondition.Bond("False", returnFalse);

            valueA.BondTo("Value", conditionEvaluator, "A");
            valueB.BondTo("Value", conditionEvaluator, "B");
            conditionEvaluator.BondTo("Value", ifCondition, "Condition");
            valueTrue.BondTo("Value", returnTrue, "Value");
            valueFalse.BondTo("Value", returnFalse, "Value");

            molecule.SetEntryPoint(entryPoint);
            if (!molecule.Validate())
                return false;
            molecule.Execute();

            string? result = molecule.GetResult<string>();
            if (result != null)
            {
                Console.WriteLine(result);
                return true;
            }
            return false;
        }

        public void TestCase()
        {
            TestMoleculeCase();
        }
    }
}
