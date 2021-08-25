import Page from '@admin/components/Page';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import imageInvertedPyramid from './images/guidance/guidance-design-standards-inverted-pyramid.png';

const DocumentationDesignStandards = () => {
  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Content design standards guide' },
      ]}
      title="Content design standards guide"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="app-content__header">
            <span className="govuk-caption-xl">
              Content design standards guide
            </span>
            <h1 className="govuk-heading-xl">Content design standards guide</h1>
          </div>
          <p>
            How to create clear and consistent content to tell people a clear
            story so they can understand our statistics and data.
          </p>

          <Accordion id="a-z">
            <AccordionSection
              id="what-is-content-design"
              heading="What is content design?"
            >
              <p>
                When we talk about content design we mean taking information and
                presenting it to people in the best possible way for their needs
                and requirements.
              </p>
              <p>
                Good content design allows people to simply and quickly do or
                find what they need from a web page or online service.
              </p>
              <p>
                For the Explore education statistics service, this means helping
                people to simply and quickly carry out the following tasks:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>finding and viewing statistics</li>
                <li>downloading data files for offline analysis</li>
                <li>building tables using the table tool</li>
                <li>
                  finding out about the methodology used in collecting and
                  presenting statistics and data
                </li>
                <li>
                  finding out who to contact to ask questions about statistics
                  and data
                </li>
              </ul>
              <p>
                Government has a tendency to publish content which is more
                focused on what it wants to say than what people want and need
                to know.
              </p>
              <p>
                This often makes our information difficult to understand and act
                on. This in turn leads to frustrated users who cannot find what
                they need or complete the tasks they want to do.
              </p>
              <h3 id="behaviour-analytics-and-feedback">
                Behaviour, analytics and feedback
              </h3>
              <p>
                However, we can avoid this by creating and formatting content
                within releases based on user behaviour, analytics and feedback.
              </p>
              <p>
                From our research, we know people find it difficult to find
                statistics and data, and to understand the related content which
                is currently published on GOV.UK under{' '}
                <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics">
                  Statistics at DfE
                </a>
              </p>
              <p>So, in the new service we know we need our content to: </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  clearly direct and signpost people to the statistics and data
                  they need
                </li>
                <li>
                  be concise and consistently formatted so it’s simple to
                  understand and easy to read
                </li>
              </ul>
              <h3 id="sound-principles-and-research">
                Sound principles and research
              </h3>
              <p>
                There’s more than one way to present content to people and the
                decisions about how we present it have to be carefully made.
              </p>
              <p>
                Writing content clearly, in plain English and optimised for the
                web helps people understand and find the information they need
                quickly and easily.
              </p>
              <p>
                This means all the content design guidance, standards and tips
                in our content-related guides are based on sound and well-known
                principles and user research.
              </p>
            </AccordionSection>
            <AccordionSection
              heading="Creating good web content"
              id="creating-good-web-content"
            >
              <p>
                People read web pages differently to how they read pages in
                print. This means creating good web content is different to
                writing good print content.
              </p>
              <p>
                Our content design guidance is based on our understanding of how
                people read and use DfE content, statistics and data, formed
                from the many hours of user research which we’ve performed (and
                continue to do so!).
              </p>
              <h3 id="meet-peoples-needs">Meet people’s needs</h3>
              <p>
                Do not add all the information and content you can to a release.
                Instead, only include what your audience needs to know to
                complete their chosen task. Nothing more.
              </p>
              <p>
                People do not usually read content unless they want information.
                So when you come to creating content, start with the same
                question every time: what do people want to know?
              </p>
              <p>Meeting that need means being:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>specific</li>
                <li>informative</li>
                <li>clear and to the point</li>
              </ul>
              <h3 id="quick-and-easy-steps">Quick and easy steps</h3>
              <p>
                Creating good content for Explore education statistics should
                mean people can take the following journey and quickly and
                easily find the statistics and data they need:
              </p>
              <ul className="govuk-list govuk-list--number">
                <li>I have a question</li>
                <li>
                  I can find the page with the answer easily – I can see it’s
                  the right page from its title and summary
                </li>
                <li>I have understood the information and have my answer</li>
                <li>I trust the information</li>
                <li>
                  I know what to do next, any fears I had have been allayed and
                  I don’t need anything else
                </li>
              </ul>
              <p>
                Good content only works if people can find what they need
                quickly, complete their task and leave without having to think
                about it too much.
              </p>
              <h3 id="easy-to-read-and-understand">
                Easy to read and understand
              </h3>
              <p>
                Good online content is easy to read and understand and we know
                from the research we’ve carried out that this means:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  being concise and using shorter sentences to drastically cut
                  the word count of our releases
                </li>
                <li>
                  creating and publishing shorter releases which contains
                  sections of around 250 words in length
                </li>
                <li>
                  using bullets and subheadings to break up the content within
                  our releases
                </li>
                <li>
                  using simple language which is quick to read and easy to
                  understand
                </li>
              </ul>
              <p>
                We know this helps people find what they need quickly and easily
                so we’ve no real excuse for putting unnecessarily complicated
                content in the way of people’s needs and requirements.
              </p>
            </AccordionSection>
            <AccordionSection
              heading="Creating specialist content"
              id="creating-specialist-content"
            >
              <p>
                Government often explains that because they’re creating
                technical or complex content for a specialist audience, they do
                not need to use plain English.
              </p>
              <p>
                This is not the case as research shows that, in fact,
                specialists prefer simple content written in plain English
                because it allows them to understand the information as quickly
                as possible.
              </p>
              <p>For example, research into legal documents found:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  80% of people preferred sentences written in clear English -
                  and the more complex the issue, the greater that preference
                  (for example, 97% preferred ‘among other things’ over the
                  Latin ‘inter alia’)
                </li>
                <li>
                  the more educated the person and the more specialist their
                  knowledge, the greater their preference for plain English
                </li>
              </ul>
              <p>
                People understand complex specialist language but do not want to
                read it if there’s an alternative. This is because specialists
                and people with the highest literacy levels and the greatest
                expertise tend to have the most to read. They do not have time
                to read through pages of complicated content.
              </p>
              <h3 id="plain-english">Plain English</h3>
              <p>
                This does not mean you cannot use technical terms in your
                content. It just means you need to explain what they mean the
                first time you use them.
              </p>
              <p>
                It’s important that our users understand our content and we
                present complicated information in as simple a way as possible.
              </p>
              <p>
                So before you start creating content for Explore education
                statistics, make sure you read this guide and:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  write to the recommended best practice and guidance it
                  contains
                </li>
                <li>
                  follow the standards set out in our online{' '}
                  <a href="/documentation/style-guide">Style guide</a>
                </li>
              </ul>
            </AccordionSection>
            <AccordionSection
              heading="Understanding your audience"
              id="understanding-your-audience"
            >
              <p>
                Your content will be most effective if you understand who you’re
                writing for.
              </p>
              <p>To understand your audience you need to know:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  how they behave, what they’re interested in or worried about -
                  so your content will catch their attention and meet their
                  needs and requirements
                </li>
                <li>
                  their vocabulary - so you can use the same terms and phrases
                  they’ll use to search for content
                </li>
              </ul>
              <h3 id="the-explore-education-statistics-audience">
                The Explore education statistics audience
              </h3>
              <p>
                Our audience is potentially anyone living in the UK who needs to
                find statistics, data and information about education.
              </p>
              <p>
                This means we must try to write and communicate in a way that
                most people understand.
              </p>
              <p>
                We know from our own and similar ONS research, very few people
                read to the end of our releases and only around 20% read past
                the first quarter.
              </p>
              <p>
                With an average page length of 17 pages, with nearly 30% more
                than 20 pages in length and 10% longer than 50 pages, of the
                60-plus publications we currently produce, the majority of them
                go unread and unused.
              </p>
              <p>This is because they are:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>too long and try to do too much in a single product</li>
                <li>difficult to read, navigate and find</li>
                <li>
                  not written in a consistent way for the widest group of users
                </li>
              </ul>
              <p>
                However, now that we know this, we’ve developed a service and
                set of content standards which will help tackle all these
                issues.
              </p>
            </AccordionSection>
            <AccordionSection
              heading="How people use content"
              id="how-people-use-content"
            >
              <p>
                Knowing how our audience uses content means you can create
                content which meets their needs and requirements.
              </p>
              <p>
                If you can do this then it means you won’t be wasting their time
                and - just as importantly - your own.
              </p>
              <p>
                The best way to do this is by using common words and working
                with natural reading behaviour.
              </p>
              <h3 id="commoon-words">Common words</h3>
              <p>
                By the time someone is 5 or 6 years old, they’ll use 2,500 to
                5,000 common words.
              </p>
              <p>
                Adults still find these words easier to recognise and understand
                than words they’ve learned since.
              </p>
              <p>
                By age 9, people have built up a ‘common words’ vocabulary of
                around 15,000 words (divided between a set of 5,000 primary and
                10,000 secondary words) which they use every day.
              </p>
              <h3 id="reading-skills">Reading skills</h3>
              <p>
                People quickly learn to read the 6,000 common words (the primary
                set that they use most). They then stop reading these words and
                start recognising their shape allowing them to read much faster.
              </p>
              <p>
                This means people do not read one word at a time and bounce
                around (especially online) anticipating words and filling them
                in.
              </p>
              <p>
                People’s brains can actually drop up to 30% of the text in front
                of them and still understand what they’re reading.
              </p>
              <p>
                This means people do not need to read every word to understand
                what is written.
              </p>
              <h3 id="reading-web-pages">Reading web pages</h3>
              <p>
                People read very differently online than they do on paper. They
                do not necessarily read top to bottom or even from word to word.
              </p>
              <p>
                Instead, they only read around 25% of web pages and, where they
                just want to complete a task as quickly as possible, they read
                even less out of impatience.
              </p>
              <p>
                Research has consistently shown that people tend to read pages
                in an{' '}
                <a href="https://drive.google.com/file/d/1yTKXvv-QUZ2pZrNNuPvPprYqCvBnyG_A/view">
                  ‘F’ shape pattern
                </a>
                . They look across the top, then down the side, reading further
                across when they find what they need.
              </p>
              <h3 id="what-does-this-mean">What does this mean?</h3>
              <p>
                This means, if we use the right words and put the most important
                first and in the right places - we can create good content for
                our users.
              </p>
              <p>
                This means you can create good content if you can meet these
                standards in terms of the language you use and the way in which
                you lay out your content.
              </p>
            </AccordionSection>
            <AccordionSection
              heading="Content design best practice, concepts and tips"
              id="content-design-best-practice-concepts-and-tips"
            >
              <p>
                This section contains a range of practical guidance and
                information about creating good content for Explore education
                statistics.
              </p>
              <p>
                It includes a combination of best practice guidelines and
                general outlines of well-known content design concepts and
                writing tips aimed at beginners.
              </p>
              <p>
                If you have any questions about or require more detailed
                guidance on anything outlined within this section refer to the
                links in the following ‘Help and support’ section of contact:{' '}
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3 id="write-to-standards-and-the-style-guide">
                Write to standards and the style guide
              </h3>
              <p>
                It’s important to read, familiarise yourself with and then
                consistently refer to the guidelines set out in this guide and
                our <a href="/documentation/style-guide">Style guide</a>.
              </p>
              <p>
                Everything they contain is based on well-known content design
                principles and a lot of user testing and, if you can stick to
                the advice they give, it will help you create clear and
                consistent content.
              </p>
              <p>
                The Style guide contains a lot of detailed information about how
                to create consistent content for Explore education statistics.
              </p>
              <p>
                However, here are some of the main points which will push your
                content in the right direction and make it simple to read and
                easy to understand:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="/documentation/style-guide#active-voice">
                    active voice
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#bullet-points">
                    bullets points
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#front-loading">
                    front-loading
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#page-length">
                    page length
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#plain-english">
                    plain English
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#punctuation">
                    punctuation
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#sentence-length">
                    sentence length
                  </a>
                </li>
                <li>
                  <a href="/documentation/style-guide#subheadings">
                    subheadings
                  </a>
                </li>
              </ul>
              <h3 id="using-the-inverted-pyramid-style">
                Using the inverted pyramid style
              </h3>
              <p>
                Writing in what is known as an ‘inverted pyramid style’ will
                help focus your content on your essential message.
              </p>
              <p>
                The inverted pyramid refers to the concept of creating content
                in order of its importance to the reader rather than in the
                traditional narrative structure (see figure below).
              </p>
              <p>
                If you can answer the following questions about the subject of
                your content within the first few paragraphs then it should
                leave with the kind of content which meets your users’ needs and
                requirements:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>Who?</li>
                <li>What?</li>
                <li>When?</li>
                <li>Where?</li>
                <li>Why?</li>
                <li>How?</li>
              </ul>
              <p>
                This should result in the conclusion of the main facts about
                your content appearing foremost within your content with other
                lesser points included in descending order of importance as you
                continue to create your content.
              </p>
              <p>
                In this way it mimics the way users use web content and is
                widely viewed as the best practice style of content creation
                (see figure below).
              </p>
              <img
                src={imageInvertedPyramid}
                className="govuk-!-width-three-quarters"
                alt=""
              />
              <p>
                You can carry out something called the ‘Two Sentence Test to see
                how successful you have been in meeting these requirements.
              </p>
              <p>
                You’ll know if you’ve been successful if you can read the first
                2 sentences of your content and take away the information you
                think your user needs and requires from your content.
              </p>
              <h3 id="do-not-duplicate-content">Do not duplicate content</h3>
              <p>
                Do not repeat content which already exists. If the content
                you’re looking to create has already been written elsewhere then
                simply link to it instead.
              </p>
              <p>
                This will cut down your word count and is something that you can
                readily do with the service as it’s no longer confined by the
                flatter structure of the existing PDF format.
              </p>
              <h3 id="avoid-redundant-introductory-words">
                Avoid redundant introductory words
              </h3>
              <p>
                These do not tend to give the user any more information than
                what they would already assume from reading the title of your
                content and finding it in the first place.
              </p>
              <p>For example:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>This set of statistics is about…</li>
                <li>The purpose of this document is…</li>
              </ul>
              <p>
                Instead, just tell users what they need to know and it will cut
                down your word count to and help you create clear and concise
                content.
              </p>
              <h3 id="plan-organise-and-question-your-content">
                Plan, organise and question your content
              </h3>
              <p>
                Break up and group your content into different topics and
                organise them under common headings.
              </p>
              <p>
                Then list the questions you think users will ask about each
                topic and put them in an order you think will make sense to your
                users.
              </p>
              <p>
                For example, bearing in mind that most users are usually
                impatient - put the question you think they’ll ask the most at
                the top.
              </p>
              <p>
                Alternatively, you could organise them in line with a process
                that your users might need to follow.
              </p>
              <p>
                Write answers to all the questions you’ve written down and then
                remove all the questions and see if what’s left flows logically.
              </p>
              <p>If not, reorder and tweak your answers until they do.</p>
              <p>
                Alternatively, finding answers to the following questions will
                help identify any gaps and make your content more user-friendly:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>Is this really what I want to say?</li>
                <li>
                  Does the information flow from the crucial to helpful and then
                  on to nice to know?
                </li>
                <li>Can I say it more clearly and concisely?</li>
                <li>Do I need to add or cut anything to make it clearer?</li>
                <li>Does it have the right tone and flow?</li>
                <li>Do I need to add any subheadings?</li>
              </ul>
              <h3 id="write-your-content-alongside-someone-else">
                Write your content alongside someone else
              </h3>
              <p>
                Consider writing your content alongside a colleague, in front of
                the same computer or on a piece of paper.
              </p>
              <p>
                This is commonly called ‘pair writing’ and you can pair up with
                anyone as it works along the lines that 2 brains work better
                than 1.
              </p>
              <p>
                Try not to work together for more than 2 hours at a time and
                find a quiet place where you can work together without
                interruption so you can get the most out of your time together.
              </p>
              <h3 id="organise-a-peer-review">Organise a peer review </h3>
              <p>
                A peer review is an opportunity for other people to comment on
                your content.
              </p>
              <p>
                It usually means getting a team of people to meet to discuss
                your content while one person takes notes.
              </p>
              <p>
                It's not easy to let other people tell you what’s wrong with
                your content so make sure you come up with some clear rules so
                you can receive some meaningful feedback.
              </p>
              <p>The 2 main rules should be something along the lines of:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  be respectful - everyone did the best job possible with the
                  knowledge they had at the time
                </li>
                <li>
                  leave your ego at the door - it's not about you...it's about
                  communicating clearly so users can understand your statistics
                  and data
                </li>
              </ul>
              <p>
                Come up with a list of simple questions to help promote
                discussion. For example:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  Do the headline messages tell the story in a clear and
                  non-technical way?
                </li>
                <li>Is the language used non-technical and accessible?</li>
                <li>
                  Are the statistics placed in the context of the real world?
                </li>
                <li>Does the title clearly describe what is in the release?</li>
                <li>Are the analysis and conclusions valid?</li>
                <li>
                  Is interpretation included that is impartial and evidence
                  based?
                </li>
                <li>Is it made clear why the statistics are produced?</li>
                <li>Does it have extra words, missing words or typos?</li>
              </ul>
              <p>
                Don't get into arguments about what you like or someone else
                likes.
              </p>
              <p>
                Instead, concentrate on whether an idea has been written to the
                standards set out in our content design guide and style guide.
              </p>
              <p>
                For more guidance on how to conduct a peer review -{' '}
                <a href="https://drive.google.com/open?id=17dMLbjU_CcFyKaDBgw26HkbcPvQTrsLd">
                  GSS Good Practice Team - Hints and Tips: Conducting a Peer
                  Review
                </a>
                .
              </p>
              <h3 id="write-like-you-would-speak">
                Write like you would speak
              </h3>
              <p>
                Good web writing should read like a face-to-face or telephone
                conversation which answers people's direct questions and let’s
                them grab and go
              </p>
              <p>
                In many cases, web sites are replacing phones. In many cases,
                the point of web content is for people to get information for
                themselves from your web site rather than having them call or
                talk to someone.
              </p>
              <p>
                So write like you would explain it to someone in a conversation
                or via a presentation and with the authority of someone who can
                actively help.
              </p>
              <p>
                You can check if you’ve been successful by simply reading your
                content out loud.
              </p>
              <p>
                Good content should also have a good rhythm and actually sound
                natural and good when it's spoken.
              </p>
              <p>
                So reading what you've written out loud is a good way to find
                out if your sentences are short enough and words clear enough.
              </p>
              <p>
                If you hesitate, stumble or have to take too many breaths when
                reading out a sentence then you’ll know something is not quite
                right.
              </p>
            </AccordionSection>
            <AccordionSection heading="Help and support" id="help-and-support">
              <p>
                The following are links to more detailed guidance on concepts
                we’ve already covered in this guide:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://gss.civilservice.gov.uk/guidances/communicating-statistics/">
                    GSS communication statistics
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/guidance/style-guide">
                    GDS style guide
                  </a>{' '}
                  - including{' '}
                  <a href="https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style#statistics">
                    GDS style guide - statistics
                  </a>
                </li>
                <li>
                  <a href="https://style.ons.gov.uk/">ONS style guide</a> -
                  including{' '}
                  <a href="https://style.ons.gov.uk/category/data-visualisation/">
                    Data visualisation
                  </a>
                </li>
                <li>
                  <a href="https://www.statisticsauthority.gov.uk/code-of-practice/">
                    UK Statistics Authority Code of Practice for Statistics
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/guidance/content-design">
                    GDS content design: planning, writing and managing content
                    manual
                  </a>
                </li>
                <li>
                  <a href="https://drive.google.com/file/d/1gKFgRapLoBahCPTUoIHZ-WBQljLuSeql/view?usp=sharing">
                    Don’t Make Me Think
                  </a>
                </li>
              </ul>
              <p>
                If you have any questions about the content design standards or
                the Style guide contact:
              </p>
              <h3>Explore education statistics team</h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </AccordionSection>
            <AccordionSection heading="Feedback" id="feedback">
              <p>
                This guidance pulls together all our current content design
                guidance for Explore education statistics but there are and will
                be gaps.
              </p>
              <p>
                Help us make it better by emailing your feedback on what you
                think is confusing, inaccurate or missing to:
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </AccordionSection>
          </Accordion>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationDesignStandards;
