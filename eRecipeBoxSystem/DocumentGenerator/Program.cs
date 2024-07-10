/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using Foundation;
using System;
using System.Diagnostics;
using System.IO;

namespace DocumentGenerator
{


    class Program
    {
        //gen document
        //https://docs.devexpress.com/OfficeFileAPI/17488/word-processing-document-api


        public static void Main()
        {
            //#TODO REFACTOR This is done quick and dirty.  good test for CoPilot

            string uiApplicationDirectory = AppSettings.GetAppSetting("UiApplicationDirectory");
            string dataModelDirectory = AppSettings.GetAppSetting("DataModelDirectory");
            string masterArtifactsFilePath = AppSettings.GetAppSetting("MasterArtifactsFilePath");
            FileInfo masterArtifactsFI = new FileInfo(masterArtifactsFilePath);
            string outputFileName = masterArtifactsFI.Name.Replace(".docx", "") + "Final.docx";
            string outputFilePath = masterArtifactsFI.DirectoryName + '\\' + outputFileName;

            using (var wordProcessor = new RichEditSoftwareArtifactServer())
            {
                wordProcessor.LoadDocument(masterArtifactsFilePath, DocumentFormat.OpenXml);
                var document = wordProcessor.Document;

                //findbusinessRulesParagragh
                Paragraph busRulesParagraphHeader = wordProcessor.FindParagraph("Business Rule Rqts", true);
                if (busRulesParagraphHeader == null)
                    throw new Exception("Paragraph 'Business Rule Requirements' is missing from document");

                Paragraph busRulesIntroParagraph = wordProcessor.FindParagraph("Business rule requirements are organized by their associated business class, one subsection for each.");
                if (busRulesIntroParagraph == null)
                    throw new Exception("Paragraph Business Rule Requirements intro is missing from document");

                ParagraphStyle classHeaderStyle = wordProcessor.GetNextHeaderLevel(busRulesParagraphHeader.Style);
                ParagraphStyle requirementHeaderStyle = document.ParagraphStyles["Normal"];
                if (requirementHeaderStyle == null)
                    throw new Exception("Paragraph style 'Normal' is required.");

                //findParagragh  one subsection for each form.
                Paragraph UIformRqtsParagraphHeader = wordProcessor.FindParagraph("UI Forms - Detailed Rqts", true);
                if (UIformRqtsParagraphHeader == null)
                    throw new Exception("Paragraph 'UI Detailed Form Requirements' is missing from document");

                Paragraph UIformRqtsIntroParagraph = wordProcessor.FindParagraph("The remaining subsections specify the detailed requirements for each UI form, one subsection for each form.");
                if (UIformRqtsIntroParagraph == null)
                    throw new Exception("Paragraph intro for UI Detailed Form Requirements is missing from document");

                ParagraphStyle UIformHeaderStyle = wordProcessor.GetNextHeaderLevel(UIformRqtsParagraphHeader.Style);
                ParagraphStyle UIrequirementHeaderStyle = document.ParagraphStyles["Normal"];
                if (UIrequirementHeaderStyle == null)
                    throw new Exception("Paragraph style 'Normal' is required.");

                document.BeginUpdate();

                //Insert business rules requirements, pulled from #RQT tags in the data model project
                //for each class
                //create heading <class>
                //add one..
                //for each rqt in class distinct/sort
                //insert the rqt
                Paragraph currInsertParagraph = busRulesIntroParagraph;
                string currBusinessClassName = null;
                var businessRulesAnnotations = CSharpSourceCodeUtils.FindAllArtifactAnnotationsInDirectory(dataModelDirectory);

                //businessRulesAnnotations = businessRulesAnnotations.OrderBy(x => x.ParentClassID).ThenBy(x => x.AssociatedDeclarationID).ToList<ArtifactAnnotation>();
                foreach (ArtifactAnnotation classDefinition in businessRulesAnnotations)
                {
                    Paragraph paragraph = null;
                    if (currBusinessClassName != classDefinition.ParentClassID)
                    {
                        paragraph = document.Paragraphs.Insert(currInsertParagraph.Range.End);
                        paragraph.Style = classHeaderStyle;
                        document.InsertText(paragraph.Range.Start, classDefinition.ClassHeaderString("Business Rules"));
                        currInsertParagraph = paragraph;
                        currBusinessClassName = classDefinition.ParentClassID;
                    }

                    paragraph = document.Paragraphs.Insert(currInsertParagraph.Range.End);
                    paragraph.Style = requirementHeaderStyle;

                    string rqtID = classDefinition.RqtID;
                    if (classDefinition.RequirementText.Count > 1)
                    {
                        //multi-line requirement case... 
                        //#RQT UsrInFilterLookup LookupEdit behavior - 
                        //#RQT UsrInFilterLookup Hit Enter key applies the selected drop down text (or new text) as a new filter term and runs the filter.
                        DocumentRange rqtIDrange = document.InsertText(paragraph.Range.Start, rqtID);
                        //bold the rqtID
                        CharacterProperties formatting = document.BeginUpdateCharacters(rqtIDrange);
                        formatting.Bold = true;
                        document.EndUpdateCharacters(formatting);
                        currInsertParagraph = paragraph;

                        foreach (string line in classDefinition.RequirementText)
                        {
                            paragraph = document.Paragraphs.Insert(currInsertParagraph.Range.End);
                            paragraph.Style = UIrequirementHeaderStyle;

                            document.InsertText(paragraph.Range.Start, line.Trim());
                            currInsertParagraph = paragraph;
                        }
                    }
                    else
                    {
                        //single-line requirement case... 
                        //#RQT UsrInEditRating User can edit MyRating for a RecipeCard in the filter results grid.
                        DocumentRange rqtIDrange = document.InsertText(paragraph.Range.Start, rqtID + " ");

                        //bold the rqtID
                        CharacterProperties formatting = document.BeginUpdateCharacters(rqtIDrange);
                        formatting.Bold = true;
                        document.EndUpdateCharacters(formatting);

                        // Insert rqt text  
                        DocumentRange rqtTextRange = document.InsertText(rqtIDrange.End, classDefinition.RequirementText[0]);

                        //unbold the rqtText
                        formatting = document.BeginUpdateCharacters(rqtTextRange);
                        formatting.Bold = false;
                        document.EndUpdateCharacters(formatting);

                        currInsertParagraph = paragraph;
                    }
                }

                //Insert UI form requirements, pulled from #RQT tags in the main UI form project
                currInsertParagraph = UIformRqtsIntroParagraph;
                string currUIformName = null;
                foreach (ArtifactAnnotation classDefinition in CSharpSourceCodeUtils.FindAllArtifactAnnotationsInDirectory(uiApplicationDirectory))
                {
                    Paragraph paragraph = null;
                    if (currUIformName != classDefinition.ParentClassID)
                    {
                        paragraph = document.Paragraphs.Insert(currInsertParagraph.Range.End);
                        paragraph.Style = UIformHeaderStyle;
                        document.InsertText(paragraph.Range.Start, classDefinition.ClassHeaderString(null));
                        currInsertParagraph = paragraph;
                        currUIformName = classDefinition.ParentClassID;
                    }

                    paragraph = document.Paragraphs.Insert(currInsertParagraph.Range.End);
                    paragraph.Style = UIrequirementHeaderStyle;

                    string rqtID = classDefinition.RqtID;
                    if (classDefinition.RequirementText.Count > 1)
                    {
                        //multi-line requirement case... 
                        //#RQT UsrInFilterLookup LookupEdit behavior - 
                        //#RQT UsrInFilterLookup Hit Enter key applies the selected drop down text (or new text) as a new filter term and runs the filter.
                        DocumentRange rqtIDrange = document.InsertText(paragraph.Range.Start, rqtID);
                        //bold the rqtID
                        CharacterProperties formatting = document.BeginUpdateCharacters(rqtIDrange);
                        formatting.Bold = true;
                        document.EndUpdateCharacters(formatting);
                        currInsertParagraph = paragraph;

                        foreach (string line in classDefinition.RequirementText)
                        {
                            paragraph = document.Paragraphs.Insert(currInsertParagraph.Range.End);
                            paragraph.Style = UIrequirementHeaderStyle;

                            document.InsertText(paragraph.Range.Start, line.Trim());
                            currInsertParagraph = paragraph;
                        }
                    }
                    else
                    {
                        //single-line requirement case... 
                        //#RQT UsrInEditRating User can edit MyRating for a RecipeCard in the filter results grid.
                        DocumentRange rqtIDrange = document.InsertText(paragraph.Range.Start, rqtID + " ");

                        //bold the rqtID
                        CharacterProperties formatting = document.BeginUpdateCharacters(rqtIDrange);
                        formatting.Bold = true;
                        document.EndUpdateCharacters(formatting);

                        // Insert rqt text  
                        DocumentRange rqtTextRange = document.InsertText(rqtIDrange.End, classDefinition.RequirementText[0]);

                        //unbold the rqtText
                        formatting = document.BeginUpdateCharacters(rqtTextRange);
                        formatting.Bold = false;
                        document.EndUpdateCharacters(formatting);

                        currInsertParagraph = paragraph;
                    }
                }
                document.EndUpdate();

                wordProcessor.SaveDocument(outputFilePath, DocumentFormat.OpenXml);
                Process.Start(new ProcessStartInfo(outputFilePath) { UseShellExecute = true });
            }
            return;

        }


        //helpful training
        //https://joshvarty.com/2015/10/25/learn-roslyn-now-part-15-the-symbolvisitor/
        //play
        //public class test : CSharpSyntaxWalker
        //{
        //    StreamWriter _log;
        //    public test(StreamWriter log) : base(SyntaxWalkerDepth.StructuredTrivia)
        //    {
        //        this._log = log;
        //    }
        //    int Tabs = 0;
        //    public int count = 0;
        //    SyntaxNode currNode;
        //    string currRegion = "";

        //    //comments
        //    //https://stackoverflow.com/questions/49843885/roslyn-get-grouped-single-line-comments
        //    public override void Visit(SyntaxNode node)
        //    {
        //        Tabs++;
        //        var indents = new String('\t', Tabs);
        //        currNode = node;
        //        //_log.WriteLine(currSK);
        //        base.Visit(node);
        //        Tabs--;
        //    }

        //    private string declIdentifier(SyntaxNode node)
        //    {
        //        var k = node.Kind();
        //        if (node.IsKind(SyntaxKind.ClassDeclaration))
        //            return ((ClassDeclarationSyntax)node).Identifier.ValueText;
        //        else if (node.IsKind(SyntaxKind.ConstructorDeclaration))
        //            return ((ConstructorDeclarationSyntax)node).Identifier.ValueText;
        //        else if (node.IsKind(SyntaxKind.MethodDeclaration))
        //            return ((MethodDeclarationSyntax)node).Identifier.ValueText;
        //        else
        //            return null;
        //    }

        //    private bool isDeclKind(SyntaxKind kind)
        //    {
        //        SyntaxKind[] declKinds = new SyntaxKind[] { SyntaxKind.ClassDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration };
        //        foreach (SyntaxKind declKind in declKinds)
        //        {
        //            if (kind == declKind)
        //                return true;
        //        }
        //        return false;
        //    }
        //    private SyntaxNode parentDeclaration(SyntaxNode node)
        //    {
        //        SyntaxNode p = node;
        //        while (p != null)
        //        {
        //            if (isDeclKind(p.Kind()))
        //                return p;
        //            p = p.Parent;
        //        }
        //        return null;
        //    }

        //    private SyntaxNode parentClassDeclaration(SyntaxNode node)
        //    {
        //        SyntaxNode p = node;
        //        while (p != null)
        //        {
        //            if (p.IsKind( SyntaxKind.ClassDeclaration))
        //                return p;
        //            p = p.Parent;
        //        }
        //        return null;
        //    }

        //    private string allParents(SyntaxNode node)
        //    {
        //        List<string> result = new List<string>();
        //        SyntaxNode p = node;
        //        while (p != null)
        //        {
        //            result.Add(p.Kind().ToString());
        //            p = p.Parent;
        //        }
        //        result.Reverse();
        //        return string.Join(".", result);
        //    }

        //    public override void VisitTrivia(SyntaxTrivia trivia)
        //    {

        //        var x = trivia.ToFullString();
        //        var xx = trivia.ToString();
        //        var z = trivia.Kind();

        //        if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
        //        {
        //            //var r = (RegionDirectiveTriviaSyntax)trivia;
        //            currRegion = trivia.ToString().Replace("#region ", "").Trim();
        //            //_log.WriteLine(xx);
        //        }
        //        if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToFullString().Contains("#RQT"))
        //        {
        //            SyntaxNode parentClassDecl = parentClassDeclaration(currNode);
        //            SyntaxNode parentDecl = parentDeclaration(currNode);
        //            string parientID = declIdentifier(parentDecl);
        //            string parientClassID = declIdentifier(parentClassDecl);
        //            _log.WriteLine($"{parientClassID}.{currRegion}.{parientID}:{x}");
        //            count++;
        //        }

        //    }
        //}

    }
}
