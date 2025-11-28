import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import React from 'react';
import { useParams } from 'react-router-dom';
import {
  subjectsForRelease1,
  PublicationSubject,
  subjectsForRelease2,
  PrototypeSubject,
  PrototypeNotification,
} from '../PrototypePublicationSubjects';
import PrototypePublicationSubject from './PrototypePublicationSubject';

interface Props {
  isCurrentReleasePublished?: boolean;
  notifications: PrototypeNotification[];
  publicationSubjects: PublicationSubject[];
  onCreateNotification: (publicationSubject: PrototypeSubject) => void;
  onEditSubject: (publicationSubject: PublicationSubject) => void;
}

const PrototypePublicationSubjectList = ({
  isCurrentReleasePublished,
  notifications,
  publicationSubjects,
  onCreateNotification,
  onEditSubject,
}: Props) => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const params: any = useParams();

  const currentRelease =
    params.id && params.id === '2022-23'
      ? 'Academic Year 2022/23'
      : 'Academic Year 2021/22';

  return (
    <>
      {publicationSubjects.length > 0 && (
        <>
          <h3 className="govuk-!-margin-top-9">Selected API data</h3>
          <Accordion id="ps">
            {publicationSubjects.map(publicationSubject => {
              const subject = subjectsForRelease1.find(
                s => s.id === publicationSubject.subjectId,
              );
              const nextSubject = publicationSubject.nextSubjectId
                ? subjectsForRelease2.find(
                    s => s.id === publicationSubject.nextSubjectId,
                  )
                : undefined;

              if (subject) {
                return (
                  <AccordionSection
                    backToTop={false}
                    heading={
                      isCurrentReleasePublished && nextSubject
                        ? nextSubject.title
                        : subject.title
                    }
                    headingTag="h3"
                    open
                    key={publicationSubject.subjectId}
                  >
                    {subject.release !== currentRelease && (
                      <div className="govuk-inset-text govuk-!-margin-top-0">
                        <h4>Publish a notification for this data set</h4>
                        <p>
                          Notifications can be published at any time. If you are
                          aware of any upcoming major changes to this data set,
                          you can use this as a channel to keep your end users
                          informed, and provide details on any elements that may
                          be changing in the upcoming new version.
                        </p>
                        <Button
                          variant="secondary"
                          onClick={() =>
                            onCreateNotification(nextSubject ?? subject)
                          }
                        >
                          Publish a notification
                        </Button>
                      </div>
                    )}

                    <PrototypePublicationSubject
                      isCurrentReleasePublished={isCurrentReleasePublished}
                      nextSubject={nextSubject}
                      notifications={notifications}
                      publicationSubject={publicationSubject}
                      subject={subject}
                      currentRelease={currentRelease}
                      onEditSubject={onEditSubject}
                    />
                  </AccordionSection>
                );
              }
              return null;
            })}
          </Accordion>
        </>
      )}
    </>
  );
};

export default PrototypePublicationSubjectList;
