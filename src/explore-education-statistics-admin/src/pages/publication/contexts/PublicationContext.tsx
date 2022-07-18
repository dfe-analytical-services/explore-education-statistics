import { MyPublication } from '@admin/services/publicationService';
import { MyRelease } from '@admin/services/releaseService';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface PublicationContextState {
  draftReleases: MyRelease[];
  publication: MyPublication;
  publicationId: string;
  publishedReleases: MyRelease[];
  scheduledReleases: MyRelease[];
  onPublicationChange: (nextPublication: MyPublication) => void;
  onReload: () => void;
}

const PublicationContext = createContext<PublicationContextState | undefined>(
  undefined,
);

interface PublicationContextProviderProps {
  children: ReactNode;
  publication: MyPublication;
  onPublicationChange: (nextPublication: MyPublication) => void;
  onReload: () => void;
}

export const PublicationContextProvider = ({
  children,
  publication,
  onPublicationChange,
  onReload,
}: PublicationContextProviderProps) => {
  const draftReleases = useMemo(() => {
    return publication.releases.filter(
      release =>
        release.approvalStatus === 'Draft' ||
        release.approvalStatus === 'HigherLevelReview',
    );
  }, [publication.releases]);

  const publishedReleases = useMemo(() => {
    return publication.releases.filter(release => release.live);
  }, [publication.releases]);

  const scheduledReleases = useMemo(() => {
    return publication.releases.filter(
      release => !release.live && release.approvalStatus === 'Approved',
    );
  }, [publication.releases]);

  const value = useMemo<PublicationContextState>(() => {
    return {
      draftReleases,
      publication,
      publicationId: publication.id,
      publishedReleases,
      scheduledReleases,
      onPublicationChange,
      onReload,
    };
  }, [
    draftReleases,
    publication,
    publishedReleases,
    scheduledReleases,
    onPublicationChange,
    onReload,
  ]);

  return (
    <PublicationContext.Provider value={value}>
      {children}
    </PublicationContext.Provider>
  );
};

export default function usePublicationContext() {
  const context = useContext(PublicationContext);

  if (!context) {
    throw new Error('Must have a parent PublicationContextProvider');
  }

  return context;
}
