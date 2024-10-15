using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public int seed = 0;
    Mesh mesh;
    private int grid_verts_per_side = 85;
    private float grid_size = 8.5f;
    // private Vector3[] verts;
    // private int [] tris;
    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);
        mesh = GetComponent<MeshFilter>().mesh; //do not remove
        
        // Mesh mesh2 = create_plane(grid_size, grid_verts_per_side, 0, 0);
        // mesh.vertices = mesh2.vertices;
        // mesh.triangles = mesh2.triangles;
        // mesh.RecalculateNormals();
        
       
        // mesh.Clear();
        // mesh.vertices = verts;
        // mesh.triangles = tris;
        // mesh.RecalculateNormals();


        // display_mesh(create_mesh());
        // display_mesh(create_grid(10, 10, 0, 0));
        // display_mesh(create_grid2(10, 1));
        mesh_to_game_object(make_grid(10,10,0,0));
        mesh_to_game_object(make_grid(10,10,10,0));
        
    }

    void OnDrawGizmos() {
        Gizmos.DrawSphere(new Vector3(0,0,0), 1);
        Gizmos.DrawSphere(new Vector3(10,0,10) , 0.5f);
        Gizmos.DrawSphere(new Vector3(9,0,9) , 0.5f);
        Gizmos.DrawSphere(new Vector3(8,0,8) , 0.5f);
        
        float grid_size = 10;
        float cell_size = 1;
        int steps = (int) (grid_size / cell_size);

        Vector3[] verts = new Vector3[(steps + 1) * (steps + 1)];
        for (int i = 0; i <= steps; i++) {
            for (int j = 0; j <= steps; j++) {
                float x_pos = cell_size * i; 
                float y_pos = cell_size * j;
                verts [i * steps + j] = new Vector3(x_pos, 0, y_pos);
                Gizmos.DrawSphere(verts[i * steps + j], 0.1f);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //must rewrite.
    private Mesh make_grid (int x_size, int y_size, float x_offset, float y_offset) {
		Mesh temp_mesh = new Mesh();
		

        Vector3[] verts = new Vector3[(x_size + 1) * (y_size + 1)];
		int vert_index = 0;
        for (int y = 0; y <= y_size; y++) {
			for (int x = 0; x <= x_size; x++) {
				verts[vert_index] = new Vector3(x + x_offset, 0, y + y_offset);   //rotate the verts based on ground, vertical, or horizontal wall
                vert_index++;
			}
		}
		

		int[] tris = new int[x_size * y_size * 6];  //2 triangles per square, 3 points per triangle

        int triangle_index = 0, vertex_index = 0;
		for (int y = 0; y < y_size; y++) {
			for (int x = 0; x < x_size; x++) {
				//triangle 1:

                tris[triangle_index] = vertex_index;    //bottom left
                tris[triangle_index + 1] = vertex_index + x_size + 1;   //top left 
                tris[triangle_index + 2] = vertex_index + 1;    //bottom right

                //triangle 2:
				tris[triangle_index + 3] = vertex_index + 1;    //bottom right
				tris[triangle_index + 4] = vertex_index + x_size + 1;   //top left
				tris[triangle_index + 5] = vertex_index + x_size + 2;   //top right

                triangle_index += 6;    //added 6 verts
                vertex_index++;         //move to right
			}
            vertex_index++;             //move to next row
		}

        temp_mesh.vertices = verts;
		temp_mesh.triangles = tris;
        return temp_mesh;
	}

    GameObject mesh_to_game_object(Mesh mesh) {
        
        GameObject s = new GameObject("terrain chunk");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = mesh;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        // color using Texture2D
        // if (terrain_selection == Terrain.Texture2D) {
        //     Texture2D texture = make_a_texture(mesh);
        //     rend.material.mainTexture = texture;
        // } else {
        //     get the renderer, attach a material that uses a vertex shader 
        //     thus, we can color each vertex and it mixes the colors. 
        //     note: this method is an alternative to using a texture 2D and potentially allows for a different gradient of colors to be made
        //     Material material = new Material(Shader.Find("Particles/Standard Surface"));
        //     rend.material = material;
        // }
       
        return s;
    }

    private Mesh create_mesh() {
        Mesh new_mesh = new Mesh();
        Vector3[] verts = new Vector3 [] {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,0), new Vector3(1,0,1)};
        int[] tris = new int [] {0 , 1, 2, 3, 2, 1};
        new_mesh.vertices = verts;
        new_mesh.triangles = tris;
        return new_mesh;
    }

    //need cell_size to divide grid_size
    private Mesh create_grid2(float grid_size, float cell_size) {
        int steps = (int) (grid_size / cell_size);

        // Vector3[] verts = new Vector3[(steps + 1) * (steps + 1)];
        // for (int i = 0; i <= steps; i++) {
        //     for (int j = 0; j <= steps; j++) {
        //         float x_pos = cell_size * i; 
        //         float y_pos = cell_size * j;
        //         verts [i * steps + j] = new Vector3(x_pos, 0, y_pos);
        //     }
        // }

        int xSize = 10, ySize = 10;
        Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		for (int i = 0, y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, y);
			}
		}
        int[] tris = new int[steps * steps * 6];
        int ntris = 0;
        // for (int i = 0; i < steps; i++) {
        //     for (int j = 0; j < steps; j++) {
        //         int tl, tr, bl, br;
        //         tl = j + (i + 1) * (steps);
        //         tr = j + (i + 1) * (steps) + 1;
        //         bl = j + i * (steps);
        //         br = j + i * (steps) + 1;
        //         MakeQuad(tl, tr, br, bl, ntris, tris);
        //         // MakeQuad(bl, br, tr, tl, ntris, tris);   //direction check
                
        //         ntris += 2;
        //     }
        // }
        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		Mesh m = new Mesh();
        m.vertices = vertices;
        mesh.triangles = triangles;
        
        // m.triangles = tris;
        return m;
    }

    // make a triangle from three vertex indices (clockwise order)
	void MakeTri(int i1, int i2, int i3, int ntris, int [] tris) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}

	// make a quadrilateral from four vertex indices (clockwise order)
	void MakeQuad(int i1, int i2, int i3, int i4, int ntris, int[] tris) {
		MakeTri (i1, i2, i3, ntris, tris);
		MakeTri (i3, i4, i1, ntris + 1, tris);
	}
    
    private Mesh create_grid(float grid_size, int grid_verts_per_side, float x_off, float y_off) {
        Vector3[] verts = new Vector3[grid_verts_per_side * grid_verts_per_side];  	                // the vertices of the mesh
	    int[] tris = new int[2 * (grid_verts_per_side - 1) * (grid_verts_per_side - 1) * 3];      	// the triangles of the mesh (triplets of integer references to vertices)
	    Mesh mesh = new Mesh();
        Color[] colors = new Color[verts.Length];;
        Vector2[] uvs = new Vector2[verts.Length];

        //generate the verticies for the plane
        for (int i = 0; i < grid_verts_per_side ; i++) {
            for (int j = 0; j < grid_verts_per_side; j++) {
                int vert_index = i * grid_verts_per_side + j;
                float x_index = grid_size / grid_verts_per_side * i + x_off;
                float y_index = grid_size / grid_verts_per_side * j + y_off;
                
                // float noise = get_perlin_noise(x_index, y_index, x_offset, y_offset);
                float noise = 0;
                // place_plant(x_index, noise, y_index);
                verts[vert_index] = new Vector3(x_index, noise, y_index);
                uvs[vert_index] = new Vector2(x_index / grid_size, (grid_size - y_index) / grid_size);
                // colors[vert_index] = get_color(noise);
            }
        }

        //generate triangles
        int ntris = 0;
        for (int i = 0; i < verts.Length - 1 - grid_verts_per_side - 1; i++) {
            if (i % grid_verts_per_side != grid_verts_per_side - 1 || i == 0) {
                int tl, tr, bl, br;
                tl = i;
                tr = i + 1;
                bl = i + grid_verts_per_side;
                br = i + grid_verts_per_side + 1;
                MakeQuad(tl, tr, br, bl, ntris, tris);
                // MakeQuad(bl, br, tr, tl, ntris, tris);   //direction check
                
                ntris += 2;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return mesh;
    }
    private void display_mesh(Mesh new_mesh) {
        // mesh = GetComponent<MeshFilter>.mesh;
        mesh.Clear();
        mesh.vertices = new_mesh.vertices;
        mesh.triangles = new_mesh.triangles;
        mesh.RecalculateNormals();
    }

    //create a new chunk of terrain as a mesh
    private Mesh create_plane(float grid_size, int grid_verts_per_side, float x_off, float y_off) {
        Vector3[] verts = new Vector3[grid_verts_per_side * grid_verts_per_side];  	                // the vertices of the mesh
	    int[] tris = new int[2 * (grid_verts_per_side - 1) * (grid_verts_per_side - 1) * 3];      	// the triangles of the mesh (triplets of integer references to vertices)
	    Mesh mesh = new Mesh();
        Color[] colors = new Color[verts.Length];;
        Vector2[] uvs = new Vector2[verts.Length];

        //generate the verticies for the plane
        for (int i = 0; i < grid_verts_per_side ; i++) {
            for (int j = 0; j < grid_verts_per_side; j++) {
                int vert_index = i * grid_verts_per_side + j;
                float x_index = grid_size / grid_verts_per_side * i + x_off;
                float y_index = grid_size / grid_verts_per_side * j + y_off;
                
                // float noise = get_perlin_noise(x_index, y_index, x_offset, y_offset);
                float noise = 0;
                // place_plant(x_index, noise, y_index);
                verts[vert_index] = new Vector3(x_index, noise, y_index);
                uvs[vert_index] = new Vector2(x_index / grid_size, (grid_size - y_index) / grid_size);
                // colors[vert_index] = get_color(noise);
            }
        }

        //generate triangles
        int ntris = 0;
        for (int i = 0; i < verts.Length - 1 - grid_verts_per_side - 1; i++) {
            if (i % grid_verts_per_side != grid_verts_per_side - 1 || i == 0) {
                int tl, tr, bl, br;
                tl = i;
                tr = i + 1;
                bl = i + grid_verts_per_side;
                br = i + grid_verts_per_side + 1;
                MakeQuad(tl, tr, br, bl, ntris, tris);
                // MakeQuad(bl, br, tr, tl, ntris, tris);   //direction check
                
                ntris += 2;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return mesh;
    }

    

}
