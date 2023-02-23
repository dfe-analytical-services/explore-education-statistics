import Button from '@common/components/Button';
import useStorageItem from '@common/hooks/useStorageItem';
import React from 'react';
import { useHistory } from 'react-router-dom';
import { PublicationSubject } from './PrototypePublicationSubjects';

const ReleaseDataPage = () => {
  const history = useHistory();
  const [, setSavedPublicationSubjects] = useStorageItem<PublicationSubject[]>(
    'publicationSubjects',
  );

  const [, setPublishedReleases] = useStorageItem<string[]>(
    'publishedReleases',
    [],
  );

  return (
    <>
      <p>Just a prototype.</p>
      <Button
        variant="secondary"
        onClick={() => {
          setSavedPublicationSubjects([]);
          setPublishedReleases([]);
          history.push('/prototypes/admin-api/data/2021-22#subjects');
        }}
      >
        Reset prototype
      </Button>
    </>
  );
};

export default ReleaseDataPage;
