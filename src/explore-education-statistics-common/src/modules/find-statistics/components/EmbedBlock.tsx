import LoadingSpinner from '@common/components/LoadingSpinner';
import useToggle from '@common/hooks/useToggle';
import IframeResizer from 'iframe-resizer-react';
import React from 'react';

interface Props {
  title: string;
  url: string;
}

const EmbedBlock = ({ title, url }: Props) => {
  const [isLoading, toggleIsLoading] = useToggle(true);

  return (
    <>
      {isLoading && <LoadingSpinner hideText text={`Loading ${title}`} />}

      <IframeResizer
        heightCalculationMethod="bodyScroll"
        src={url}
        style={{ border: 0, minWidth: '100%', width: '1px' }}
        title={title}
        onLoad={toggleIsLoading.off}
      />
    </>
  );
};

export default EmbedBlock;
