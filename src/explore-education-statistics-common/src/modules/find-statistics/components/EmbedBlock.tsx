import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import IframeResizer from 'iframe-resizer-react';
import React from 'react';

export const allowedEmbedDomains = [
  'https://department-for-education.shinyapps.io',
];

interface Props {
  title: string;
  url: string;
}

const EmbedBlock = ({ title, url }: Props) => {
  const [isLoading, toggleIsLoading] = useToggle(true);
  const [isInitialised, toggleIsInitialised] = useToggle(false);

  return (
    <>
      {isLoading && <LoadingSpinner hideText text={`Loading ${title}`} />}
      {!isLoading && !isInitialised && (
        <WarningMessage>Could not load iframe.</WarningMessage>
      )}
      <IframeResizer
        heightCalculationMethod="lowestElement"
        src={url}
        style={{ border: 0, minWidth: '100%', width: '1px' }}
        title={title}
        checkOrigin={allowedEmbedDomains}
        onInit={toggleIsInitialised.on}
        onLoad={toggleIsLoading.off}
      />
    </>
  );
};

export default EmbedBlock;
