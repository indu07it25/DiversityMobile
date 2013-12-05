using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Diagnostics;

namespace DiversityPhone.ViewModels.Elements
{
    public class VMFactory : ICreateViewModels
    {
        readonly IVocabularyService Vocabulary;

        public VMFactory(IVocabularyService Voc)
        {
            this.Vocabulary = Voc;
        }

        public IElementVM<T> CreateVM<T>(T model)
        {
            if (model is EventSeries) { return new EventSeriesVM(model as EventSeries) as IElementVM<T>; }
            if (model is Event) { return new EventVM(model as Event) as IElementVM<T>; }
            if (model is EventProperty) { throw new NotImplementedException("Event Property VM"); }
            if (model is Specimen) { return new SpecimenVM(model as Specimen) as IElementVM<T>; }
            if (model is IdentificationUnit) { return new IdentificationUnitVM(model as IdentificationUnit) as IElementVM<T>; }
            if (model is IdentificationUnitAnalysis) { return new IdentificationUnitAnalysisVM(model as IdentificationUnitAnalysis, Vocabulary) as IElementVM<T>; }
            if (model is MultimediaObject) { return new MultimediaObjectVM(model as MultimediaObject) as IElementVM<T>; }


            Debugger.Break();
            throw new ArgumentException("Invalid Model class");
        }
    }
}
