import React, { Component } from 'react';
import axios from 'axios';
import DataList from '../components/datalist';
import Title from '../components/title';

class Topic extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        axios.get(`http://localhost:5010/api/Topic/${handle}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        const publications = [{
            id: "cbbd299f-8297-44bc-92ac-558bcf51f8ad",
            title: "Pupil absence in schools in England"
        }];
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label={data.title} />
                    <h2>Publications</h2>
                    <DataList data={publications} linkIdentifier="publication" />
                </div>
            </div>
        );
    }
}

export default Topic;