interface PdfViewerProps {
  src: string;
  title?: string;
}

export const PdfViewer = ({ src, title }: PdfViewerProps) => {
  return (
    <div className="w-full border rounded">
      {/* react-pdf integration will be added here */}
      <object
        data={src}
        type="application/pdf"
        width="100%"
        height="600px"
      >
        <p>
          Tu navegador no soporta PDFs. 
          <a href={src} target="_blank" rel="noopener noreferrer">Descargar PDF</a>
        </p>
      </object>
      {title && <p className="mt-2 text-sm text-gray-600">{title}</p>}
    </div>
  );
};
