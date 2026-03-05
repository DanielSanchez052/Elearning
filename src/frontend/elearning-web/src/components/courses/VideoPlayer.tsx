interface VideoPlayerProps {
  src: string;
  title?: string;
}

export const VideoPlayer = ({ src, title }: VideoPlayerProps) => {
  return (
    <div className="w-full bg-black rounded">
      {/* Video.js integration will be added here */}
      <video 
        controls 
        className="w-full"
        controlsList="nodownload"
      >
        <source src={src} type="video/mp4" />
        Tu navegador no soporta video HTML5.
      </video>
      {title && <p className="mt-2 text-sm text-gray-600">{title}</p>}
    </div>
  );
};
