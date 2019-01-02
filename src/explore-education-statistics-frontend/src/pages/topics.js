import React, { Component } from 'react';
import axios from 'axios';
import DataList from '../components/datalist';
import Title from '../components/title';

class Topics extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        axios.get('http://localhost:5010/api/topic')
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label='Topics' />
                    <DataList data={data} linkIdentifier='topic' />
                </div>
            </div>
        );
    }
}

export default Topics;