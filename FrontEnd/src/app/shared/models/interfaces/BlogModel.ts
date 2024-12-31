export class Blogmodel {
    id: number | undefined;
    date: Date | undefined;
    author: string | undefined;
    title: string | undefined;
    blogIntroduction: string | undefined;
    minutes: number | undefined;
    readCount?: number | null;
    tags: string | undefined;
    references?: string | null;
    image: string | undefined;
}