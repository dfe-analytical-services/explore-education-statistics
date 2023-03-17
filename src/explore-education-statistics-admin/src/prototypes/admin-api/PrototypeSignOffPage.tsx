import Button from '@common/components/Button';
import useToggle from '@common/hooks/useToggle';
import Link from '@admin/components/Link';
import useStorageItem from '@common/hooks/useStorageItem';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface MatchProps {
  id: string;
}

const PrototypeSignOffPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const [published, setPublished] = useToggle(false);

  const [publishedReleases, setPublishedReleases] = useStorageItem<string[]>(
    'publishedReleases',
    [],
  );

  if (match.params.id === '2021-22') {
    if (published) {
      return (
        <>
          <h3>2022/23 release created</h3>
          <p>
            <Link to="/prototypes/admin-api/data/2022-23#subjects">
              Go to API Datasets for 2022/23 release
            </Link>
          </p>
        </>
      );
    }
    return (
      <Button
        onClick={() => {
          setPublishedReleases([match.params.id]);
          setPublished.on();
        }}
      >
        Publish 2021/22 release and create new release for 2022/23
      </Button>
    );
  }

  if (published) {
    return (
      <>
        <h3>2022/23 release published</h3>
        <p>
          <Link to="/prototypes/admin-api/data/2022-23#subjects">
            Back to API Datasets for 2022/23 release
          </Link>
        </p>
      </>
    );
  }

  return (
    <Button
      onClick={() => {
        setPublishedReleases(
          publishedReleases
            ? [...publishedReleases, match.params.id]
            : [match.params.id],
        );
        setPublished.on();
      }}
    >
      Publish 2022/23 release
    </Button>
  );
};

export default PrototypeSignOffPage;
