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
using System.Collections.Generic;

namespace DocumentGenerator
{
    public class RichEditSoftwareArtifactServer : RichEditDocumentServer
    {
        private List<ParagraphStyle> documentStyles = null;
        public ParagraphStyle GetNextHeaderLevel(ParagraphStyle paragraphStyle)
        {

            Document document = this.Document;
            if (documentStyles == null)
            {
                documentStyles = new List<ParagraphStyle>();
                var normal = document.ParagraphStyles["Normal"];
                documentStyles.Add(normal);

                var nextLevel = document.ParagraphStyles["Heading 1"];

                int counter = 1;
                while (nextLevel != null)
                {
                    documentStyles.Add(nextLevel);
                    counter++;
                    nextLevel = document.ParagraphStyles[$"Heading {counter}"];
                }
            }
            int paragraphStyleIndex = -1;
            //find
            for (int i = 0; i < documentStyles.Count; i++)
            {
                ParagraphStyle ps = documentStyles[i];
                if (ps == paragraphStyle)
                {
                    paragraphStyleIndex = i;
                    break;
                }
            }
            if (paragraphStyleIndex < 0)
                throw new System.Exception($"unable to find style {paragraphStyle.Name}");

            //get next one
            return documentStyles[paragraphStyleIndex + 1];
        }

        public Paragraph FindParagraph(string withText, bool isHeading = false)
        {
            Document document = this.Document;
            foreach (Paragraph paragraph in document.Paragraphs)
            {
                DocumentRange r = paragraph.Range;
                string paragraphText = document.GetText(r);
                //#TRICKY  use ends with because headings are prefixed with the para number
                if (paragraphText.EndsWith(withText))
                {
                    if (!isHeading || (isHeading && paragraph.Style.Name.StartsWith("Heading ")))
                        return paragraph;
                }
            }
            return null;
        }
    }
}
