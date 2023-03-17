import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Link from '@admin/components/Link';
import React from 'react';
import { useParams } from 'react-router-dom';
import {
  subjectsForRelease1,
  PublicationSubject,
  subjectsForRelease2,
} from '../PrototypePublicationSubjects';

interface Props {
  isCurrentReleasePublished?: boolean;
  publicationSubjects: PublicationSubject[];
  // onEditTitle: (publicationSubject: PublicationSubject) => void;
  onEditSubject: (publicationSubject: PublicationSubject) => void;
}

const PrototypePublicationSubjectList = ({
  isCurrentReleasePublished,
  publicationSubjects,
  // onEditTitle,
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
          <h3 className="govuk-!-margin-top-9">Selected API Datasets</h3>
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
                    heading={
                      isCurrentReleasePublished && nextSubject
                        ? nextSubject.title
                        : subject.title
                    }
                    headingTag="h3"
                    open
                    key={publicationSubject.title}
                  >
                    <SummaryList>
                      <SummaryListItem
                        term={
                          subject.release === currentRelease
                            ? 'Next dataset to publish'
                            : 'Current dataset'
                        }
                      >
                        {isCurrentReleasePublished && nextSubject
                          ? nextSubject.title
                          : subject.title}
                      </SummaryListItem>
                      <SummaryListItem
                        term={
                          subject.release === currentRelease
                            ? 'Next release to publish'
                            : 'Current release'
                        }
                      >
                        {isCurrentReleasePublished && nextSubject
                          ? nextSubject.release
                          : subject.release}
                      </SummaryListItem>
                      {nextSubject && !isCurrentReleasePublished && (
                        <>
                          <SummaryListItem term="Next dataset to publish">
                            {nextSubject.title}
                          </SummaryListItem>
                          <SummaryListItem term="Next release to publish">
                            {nextSubject.release}
                          </SummaryListItem>
                        </>
                      )}
                      {!isCurrentReleasePublished && (
                        <SummaryListItem term="Actions">
                          <ButtonGroup className="dfe-justify-content--flex-end">
                            {subject.release === currentRelease && (
                              <ButtonText
                                onClick={() =>
                                  onEditSubject(publicationSubject)
                                }
                              >
                                Change dataset to publish
                              </ButtonText>
                            )}
                            {nextSubject && (
                              <>
                                <ButtonText>Edit next dataset</ButtonText>
                                <ButtonText>Remove next dataset</ButtonText>
                              </>
                            )}

                            {/* <ButtonText
                              onClick={() => onEditTitle(publicationSubject)}
                            >
                              Edit dataset title
                            </ButtonText> */}

                            {subject.release !== currentRelease &&
                              !nextSubject && (
                                <Link
                                  className="govuk-button"
                                  to={`./2022-23/prepare-subject/${publicationSubject.subjectId}`}
                                >
                                  Link a dataset for next release
                                </Link>
                              )}

                            {subject.release === currentRelease && (
                              <ButtonText variant="warning">Delete</ButtonText>
                            )}
                          </ButtonGroup>
                        </SummaryListItem>
                      )}
                    </SummaryList>
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
