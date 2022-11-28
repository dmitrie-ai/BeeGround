import numpy as np
import matplotlib.pyplot as plt
from scipy.ndimage import gaussian_filter
class Cue:
    def __init__(self, center,sigma,max):
        self.center = center # (x,y) of cue with (0,0) at top left corner
        self.sigma = sigma # standard deviation. Bigger results in bigger cue
        self.max = max # value at center of cue

def create_map(out_file, arena_size, cue_list):
    '''
    Create a temp array
    :param str out_file:
    :param int[] arena_size:
    :param Cue[] cue_list: list of Cue objects
    :return:
    '''

    a = build_array(arena_size=arena_size, cue_list= cue_list)
    save_to_file(out_file,a)

def save_to_file(out_file,a):
    '''
    save the array to fil ein the format required by BEEGROUND
    :param out_file: path to output file from root directory
    :param a: np.ndarray
    '''
    with open(out_file,"w") as f:
        for row_arr in a:
            f.write(",".join(row_arr.astype("str")))
            f.write("\n")

def gauss2d(x,y,center,sigma,max):
    '''
    Get the value of a Symmetrical 2 dimensional gaussian distribution at a particular x and y
    :param x: x coordinate
    :param y: y coordinate
    :param center: (x of center, y of center)
    :param sigma: standard deviation of the gaussian
    :param max: The value at center
    :return: float
    '''
    x_0,y_0 = center
    return max* np.exp(-((((x - x_0) ** 2) + ((y - y_0) ** 2))/(2 * sigma ** 2)))

def build_array(arena_size,cue_list):
    '''

    :param (int,int) arena_size: size of arena
    :param Cue[] cue_list: list of Cue objects defining what cues to be generated
    :return: np.ndarray of size arena_size*10 because in BEEGROUND every unit is diivded into 10 subiunits
    '''
    arena_size = (arena_size[0]*10,arena_size[1]*10)
    arr = np.zeros(arena_size,dtype="uint")
    for x in range(arena_size[0]):
        for y in range(arena_size[1]):
            # add the intensity values of each cue at (x,y)
            value= 0
            for cue in cue_list:
                center= (cue.center[0]*10,cue.center[1]*10) # BEEGROUND devides every unit into 10 subunits
                sigma= cue.sigma*10
                value +=gauss2d(x,y,center,sigma,cue.max)
            arr[x][y] = int(value)
    return arr

def display(arr):
    plt.imshow(arr,interpolation="nearest",cmap="hot")
    plt.show()

# list of cues
cue_list = [Cue(center = (24,24),sigma= 6,max= 255)]
#if you want to visualise the map first
# a= build_array((50,50),cue_list)
# display(a)
# main funciton
create_map(out_file = "5050_single_med.txt",arena_size = (50,50),cue_list= cue_list)

