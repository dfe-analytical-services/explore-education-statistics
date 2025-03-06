import React, { createContext, useContext, useState, useCallback } from "react";
import downloadFile from "@common/utils/file/downloadFile";
import { useCurrentPng, useGenerateImage } from "./ChartsToPNG";
import ButtonText from "./ButtonText";

// Create Context with Default Value
export const ExportButtonContext = createContext<React.RefObject<HTMLDivElement> | null>(null);

export const useRefContext = () => {
  const context = useContext(ExportButtonContext);
  if (!context) {
    throw new Error("useRefContext must be used within a RefProvider");
  }
  return context;
};

const RefComponent = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [getClipboardPng] = useCurrentPng();

  const [getDivJpeg] = useGenerateImage({
    quality: 0.8,
    type: "image/jpeg",
  });

  const handleDivDownload = useCallback(async () => {
    const jpeg = await getDivJpeg();
    if (jpeg) {
      downloadFile(jpeg, 'test.jpg');
    }
  }, [getDivJpeg]);

  const handleCopyToClipboard = useCallback(async () => {
    await getClipboardPng(async (blob) => {
      try {
        if (blob) {
          const clipboardItems: ClipboardItem[] = [];
          const clipboardItem = new ClipboardItem({ [blob.type]: blob });
          clipboardItems.push(clipboardItem);
          await navigator.clipboard.write(clipboardItems);
        }
      } catch (e) {
        console.error(e);
      }
    });
  }, [getClipboardPng]);

  return (
    <div className="flex flex-col items-center space-y-2">
      <div className="relative">
        <ButtonText
          className="px-4 py-2 bg-blue-500 text-white rounded-md"
          onClick={() => setIsOpen(!isOpen)}
        >
          Export
        </ButtonText>
        {isOpen && (
          <div className="absolute top-full left-0 mt-2 bg-white border rounded-md shadow-md">
            <ButtonText
              className="govuk-!-margin-left-2"
              onClick={handleDivDownload}
            >
              Download as png
            </ButtonText>
            <br />
            <ButtonText
              className="govuk-!-margin-left-2"
              onClick={handleCopyToClipboard}
            >
              Copy chart to clipboard
            </ButtonText>
          </div>
        )}
      </div>
    </div>
  );
};

export default RefComponent;