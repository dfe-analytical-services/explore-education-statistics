import Link from '@admin/components/Link';
import { Publication } from '@admin/services/publicationService';
import methodologyQueries from '@admin/queries/methodologyQueries';
import publicationQueries from '@admin/queries/publicationQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { ReleaseType } from '@common/services/types/releaseType';
import { Publication as ContentPublication } from '@common/services/publicationService';
import React, { ReactNode } from 'react';
import { useQuery } from '@tanstack/react-query';

interface Props {
  publication: Publication | ContentPublication;
  releaseType: ReleaseType;
}

export default function ReleaseTableToolInfoWrapper({
  publication,
  releaseType,
}: Props) {
  const { data: methodologies, isLoading: isLoadingMethodologies } = useQuery({
    ...methodologyQueries.listLatestMethodologyVersions(publication.id),
    staleTime: Infinity,
  });

  const { data: contact, isLoading: isLoadingContact } = useQuery({
    ...publicationQueries.getContact(publication.id),
    staleTime: Infinity,
  });

  const {
    data: externalMethodology = undefined,
    isLoading: isLoadingExternalMethodology,
  } = useQuery({
    ...publicationQueries.getExternalMethodology(publication.id),
    staleTime: Infinity,
  });

  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      methodologies?.map(methodology => methodology.title) ?? [];

    if (externalMethodology) {
      links.push(
        <Link key={externalMethodology.url} to={externalMethodology.url}>
          {externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <LoadingSpinner
      loading={
        isLoadingContact ||
        isLoadingMethodologies ||
        isLoadingExternalMethodology
      }
    >
      <TableToolInfo
        contactDetails={contact}
        methodologyLinks={getMethodologyLinks()}
        releaseLink={<span>{publication.title}</span>}
        releaseType={releaseType}
      />
    </LoadingSpinner>
  );
}
