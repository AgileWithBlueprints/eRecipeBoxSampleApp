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
using Foundation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocumentGenerator
{
    //#TODO REFACTOR This is done quick and dirty.  good test for CoPilot
    public class CSharpSourceCodeUtils
    {
        static public List<ArtifactAnnotation> FindAllArtifactAnnotationsInDirectory(string rootDirectory)
        {
            List<ArtifactAnnotation> result = new List<ArtifactAnnotation>();
            DirectoryInfo di = new DirectoryInfo(rootDirectory);
            if (!di.Exists)
                throw new Exception($"{rootDirectory} not found.");
            string[] files = Directory.GetFiles(rootDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                result.AddRange(FindAllArtifactAnnotationsInSourceFile(file));
            }
            List<ArtifactAnnotation> SortedList = result.OrderBy(o => o.ParentClassID).ThenBy(o => o.RqtID).ToList();
            return SortedList;
        }

        static public List<ArtifactAnnotation> FindAllArtifactAnnotationsInSourceFile(string sourceFilePath)
        {
            string csCode = File.ReadAllText(sourceFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(csCode);
            ArtifactAnnotationWalker t = new ArtifactAnnotationWalker();
            SyntaxNode root = tree.GetRoot();
            t.Visit(root);
            return t.Annotations;
        }

        static public void FindAllAnnotationsInSourceFile(string sourceFilePath)
        {
            string csCode = File.ReadAllText(sourceFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(csCode);
            ArtifactAnnotationWalker t = new ArtifactAnnotationWalker();
            SyntaxNode root = tree.GetRoot();
            t.Visit(root);
            //return t.Annotations;
        }
    }

    public class ArtifactAnnotation
    {
        public string ParentClassID { get; set; }
        private string fRegionID;
        public string RegionID { get { if (fRegionID == null) return ""; else return fRegionID; } set { this.fRegionID = value; } }

        private string fAssociatedDeclarationID;
        public string AssociatedDeclarationID { get { if (fAssociatedDeclarationID == null) return ""; else return fAssociatedDeclarationID; } set { this.fAssociatedDeclarationID = value; } }

        private List<string> fRequirementText = new List<string>();
        public IList<string> RequirementText { get { return fRequirementText; } }

        public string RqtID { get; set; }

        public string ClassHeaderString(string suffix)
        {
            string suff = suffix ?? "";
            return $"{ParentClassID} {suff}".Trim();
        }
    }

    //good training
    //https://joshvarty.com/2015/10/25/learn-roslyn-now-part-15-the-symbolvisitor/
    public class ArtifactAnnotationWalker : CSharpSyntaxWalker
    {
        public ArtifactAnnotationWalker() : base(SyntaxWalkerDepth.StructuredTrivia)
        {
        }

        public List<ArtifactAnnotation> Annotations { get { return this.rqtIdAnnotations.Values.ToList<ArtifactAnnotation>(); } }
        SyntaxNode currNode;
        string currRegion = "";

        //comments
        //https://stackoverflow.com/questions/49843885/roslyn-get-grouped-single-line-comments
        public override void Visit(SyntaxNode node)
        {
            currNode = node;
            string nodeStr = node.ToString().ToLower();
            if (node.IsKind(SyntaxKind.Attribute) && nodeStr.Contains("nullable") && nodeStr.Contains("false"))
            {
                ArtifactAnnotation newAnnotation = new ArtifactAnnotation();
                SyntaxNode parentClassDecl = ParentClassDeclaration(node);
                SyntaxNode parentDecl = ParentDeclaration(node);
                string propertyDeclarationID = (string)((Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax)parentDecl).Identifier.Value;
                string parientClassID = DeclIdentifier(parentClassDecl);
                newAnnotation.ParentClassID = parientClassID;
                newAnnotation.RegionID = currRegion;
                newAnnotation.AssociatedDeclarationID = propertyDeclarationID;
                newAnnotation.RequirementText.Add($"{newAnnotation.AssociatedDeclarationID} is required.");
                string rqtID = PrimitiveUtils.Abbreviate(newAnnotation.AssociatedDeclarationID + "Required");
                newAnnotation.RqtID = rqtID;
                rqtIdAnnotations[rqtID] = newAnnotation;
            }
            base.Visit(node);
        }

        protected string DeclIdentifier(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.ClassDeclaration))
                return ((ClassDeclarationSyntax)node).Identifier.ValueText;
            else if (node.IsKind(SyntaxKind.ConstructorDeclaration))
                return ((ConstructorDeclarationSyntax)node).Identifier.ValueText;
            else if (node.IsKind(SyntaxKind.MethodDeclaration))
                return ((MethodDeclarationSyntax)node).Identifier.ValueText;
            else
                return null;
        }

        protected bool IsDeclKind(SyntaxKind kind)
        {
            SyntaxKind[] declKinds = new SyntaxKind[] { SyntaxKind.ClassDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration };
            foreach (SyntaxKind declKind in declKinds)
            {
                if (kind == declKind)
                    return true;
            }
            return false;
        }
        protected SyntaxNode ParentDeclaration(SyntaxNode node)
        {
            SyntaxNode p = node;
            while (p != null)
            {
                if (IsDeclKind(p.Kind()))
                    return p;
                p = p.Parent;
            }
            return null;
        }

        protected SyntaxNode ParentClassDeclaration(SyntaxNode node)
        {
            SyntaxNode p = node;
            while (p != null)
            {
                if (p.IsKind(SyntaxKind.ClassDeclaration))
                    return p;
                p = p.Parent;
            }
            return null;
        }

        protected string AllParents(SyntaxNode node)
        {
            List<string> result = new List<string>();
            SyntaxNode p = node;
            while (p != null)
            {
                result.Add(p.Kind().ToString());
                p = p.Parent;
            }
            result.Reverse();
            return string.Join(".", result);
        }

        private readonly Dictionary<string, ArtifactAnnotation> rqtIdAnnotations = new Dictionary<string, ArtifactAnnotation>();

        public override void VisitTrivia(SyntaxTrivia trivia)
        {

            var triviaText = trivia.ToFullString();
            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                string id = trivia.ToString().Replace("#region ", "").Trim();
                if (string.IsNullOrWhiteSpace(id))
                    currRegion = null;
                else
                    currRegion = id;
            }
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToFullString().Contains("#RQT"))
            {
                string rqtIdText = triviaText.Replace("//#RQT", "").Trim();
                var result = Regex.Match(rqtIdText, @"^([\w\-]+)");
                string rqtID = result.Value;
                string rqtText = rqtIdText.Substring(result.Length).Trim();
                ArtifactAnnotation annotation;
                if (!rqtIdAnnotations.ContainsKey(rqtID))
                {
                    annotation = new ArtifactAnnotation();
                    SyntaxNode parentClassDecl = ParentClassDeclaration(currNode);
                    SyntaxNode parentDecl = ParentDeclaration(currNode);
                    string parientID = DeclIdentifier(parentDecl);
                    string parientClassID = DeclIdentifier(parentClassDecl);
                    annotation.ParentClassID = parientClassID;
                    annotation.RegionID = currRegion;
                    annotation.AssociatedDeclarationID = parientID;
                    annotation.RqtID = rqtID;
                    rqtIdAnnotations[rqtID] = annotation;
                }
                else
                    annotation = rqtIdAnnotations[rqtID];

                annotation.RequirementText.Add(rqtText);

            }
        }
    }

}
