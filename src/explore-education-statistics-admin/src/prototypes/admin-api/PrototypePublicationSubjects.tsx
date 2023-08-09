import useStorageItem from '@common/hooks/useStorageItem';
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import PrototypePublicationSubjectList from './components/PrototypePublicationSubjectList';
import PrototypeAddPublicationSubject from './components/PrototypeAddPublicationSubject';
import PrototypeEditPublicationSubjectTitle from './components/PrototypeEditPublicationSubjectTitle';
import PrototypeEditPublicationSubject from './components/PrototypeEditPublicationSubject';
import { PrototypeNextSubjectContextProvider } from './contexts/PrototypeNextSubjectContext';

export interface PublicationSubject {
  title: string;
  subjectId: string;
  nextSubjectId?: string;
}

export interface PrototypeSubject {
  id: string;
  title: string;
  release: string;
  version: string;
}

export const subjectsForRelease1: PrototypeSubject[] = [
  {
    id: 'id1',
    title: 'Children in need and episodes of need by local authority',
    release: 'Academic Year 2021/22',
    version: '1.0',
  },
  {
    id: 'id2',
    title:
      "Referrals and re-referrals to children's social care services by local authority",
    release: 'Academic Year 2021/22',
    version: '1.0',
  },
];
export const subjectsForRelease2: PrototypeSubject[] = [
  {
    id: 'id3',
    title: 'Children in need and episodes of need by local authority',
    release: 'Academic Year 2022/23',
    version: '1.1',
  },
  {
    id: 'id4',
    title: 'A different subject',
    release: 'Academic Year 2022/23',
    version: '1.1',
  },
  {
    id: 'id5',
    title:
      "Referrals and re-referrals to children's social care services by local authority",
    release: 'Academic Year 2022/23',
    version: '1.1',
  },
];

const PrototypePublicationSubjects = () => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const params: any = useParams();

  const [publishedReleases] = useStorageItem<string[]>('publishedReleases', []);
  const currentRelease = params.id ?? '2021-22';
  const isCurrentReleasePublished = publishedReleases?.includes(currentRelease);

  // save in local storage as no db
  const [
    savedPublicationSubjects,
    setSavedPublicationSubjects,
  ] = useStorageItem<PublicationSubject[]>('publicationSubjects', []);

  const [publicationSubjects, setPublicationSubjects] = useState<
    PublicationSubject[]
  >(savedPublicationSubjects ?? []);

  // update local storage if they change
  useEffect(() => {
    setSavedPublicationSubjects(publicationSubjects);
  }, [publicationSubjects, setSavedPublicationSubjects]);

  const [subjectToEdit, setSubjectToEdit] = useState<
    PublicationSubject | undefined
  >(undefined);

  const [subjectToChange, setSubjectToChange] = useState<
    PublicationSubject | undefined
  >(undefined);

  const subjects =
    currentRelease === '2021-22' ? subjectsForRelease1 : subjectsForRelease2;

  if (subjectToEdit) {
    return (
      <PrototypeEditPublicationSubjectTitle
        publicationSubject={subjectToEdit}
        onClose={() => setSubjectToEdit(undefined)}
        onSubmit={updatedPublicationSubject => {
          const updated = publicationSubjects.map(subject =>
            subject.title === subjectToEdit.title
              ? updatedPublicationSubject
              : subject,
          );
          setPublicationSubjects(updated);
          setSubjectToEdit(undefined);
        }}
      />
    );
  }

  if (subjectToChange) {
    return (
      <PrototypeEditPublicationSubject
        publicationSubject={subjectToChange}
        subjects={subjects}
        onClose={() => setSubjectToChange(undefined)}
        onSubmit={updatedPublicationSubject => {
          const updated = publicationSubjects.map(subject =>
            subject.title === subjectToChange.title
              ? updatedPublicationSubject
              : subject,
          );
          setPublicationSubjects(updated);
          setSubjectToChange(undefined);
        }}
      />
    );
  }

  return (
    <PrototypeNextSubjectContextProvider
      locations={{
        newItems: [],
        mappedItems: [],
        unmappedItems: [],
        noMappingItems: [],
      }}
      filters={{
        newItems: [],
        mappedItems: [],
        unmappedItems: [],
        noMappingItems: [],
      }}
      indicators={{
        newItems: [],
        mappedItems: [],
        unmappedItems: [],
        noMappingItems: [],
      }}
      versionType="minor"
    >
      <PrototypeAddPublicationSubject
        isCurrentReleasePublished={isCurrentReleasePublished}
        subjects={subjects.filter(subject => {
          return !publicationSubjects.find(ps => ps.subjectId === subject.id);
        })}
        onSubmit={newPublicationSubject => {
          const updatedPublicationSubjects = [
            ...publicationSubjects,
            newPublicationSubject,
          ];
          setPublicationSubjects(updatedPublicationSubjects);
        }}
      />
      <PrototypePublicationSubjectList
        isCurrentReleasePublished={isCurrentReleasePublished}
        publicationSubjects={publicationSubjects}
        onEditSubject={setSubjectToChange}
      />
    </PrototypeNextSubjectContextProvider>
  );
};
export default PrototypePublicationSubjects;
