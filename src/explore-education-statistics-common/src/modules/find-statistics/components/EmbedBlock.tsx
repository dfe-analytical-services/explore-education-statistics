import LoadingSpinner from '@common/components/LoadingSpinner';
import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import IframeResizer from 'iframe-resizer-react';
import React from 'react';

interface Props {
  title: string;
  url: string;
}

const EmbedBlock = ({ title, url }: Props) => {
  const [isLoading, toggleIsLoading] = useToggle(true);

  // Only render the iframe client-side, as an iframe that loads before
  // hydration would never fire `onLoad`, so the spinner would never hide.
  const { isMounted } = useMounted();

  return (
    <>
      {isLoading && <LoadingSpinner hideText text={`Loading ${title}`} />}

      {isMounted && (
        <IframeResizer
          src={url}
          style={{ border: 0, minWidth: '100%', width: '1px' }}
          title={title}
          onLoad={toggleIsLoading.off}
        />
      )}
    </>
  );
};

export default EmbedBlock;
